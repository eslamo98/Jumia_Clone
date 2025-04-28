using AutoMapper;
using Jumia_Clone.Data;
using Jumia_Clone.Models.Constants;
using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Models.DTOs.OrderDTOs;
using Jumia_Clone.Models.Entities;
using Jumia_Clone.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;

namespace Jumia_Clone.Repositories.Implementation
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderRepository> _logger;

        public OrderRepository(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<OrderRepository> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersAsync(PaginationDto pagination)
        {
            try
            {
                var orders = await _context.Orders
                    .Include(o => o.SubOrders)
                        .ThenInclude(so => so.OrderItems)
                    .OrderByDescending(o => o.CreatedAt)
                    .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                    .Take(pagination.PageSize)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<OrderDto>>(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders");
                throw;
            }
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByCustomerIdAsync(int customerId, PaginationDto pagination)
        {
            try
            {
                var orders = await _context.Orders
                    .Where(o => o.CustomerId == customerId)
                    .Include(o => o.SubOrders)
                        .ThenInclude(so => so.OrderItems)
                    .OrderByDescending(o => o.CreatedAt)
                    .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                    .Take(pagination.PageSize)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<OrderDto>>(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders for customer {CustomerId}", customerId);
                throw;
            }
        }
        public async Task<(bool Success, string Message)> CancelOrderAsync(int orderId, int customerId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Get the order with all related data
                var order = await _context.Orders
                    .Include(o => o.SubOrders)
                        .ThenInclude(so => so.OrderItems)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId );

                if (order == null)
                    return (false, "Order not found or does not belong to this customer");

                // Check if order can be canceled (only pending or processing orders can be canceled)
                if (order.OrderStatus != OrderStatus.Pending && order.OrderStatus != OrderStatus.Processing)
                    return (false, $"Cannot cancel order with status '{order.OrderStatus}'");

                // Update order status
                order.OrderStatus = OrderStatus.Canceled;
                order.UpdatedAt = DateTime.UtcNow;

                // Update all suborders status
                foreach (var subOrder in order.SubOrders)
                {
                    subOrder.Status = OrderStatus.Canceled;
                   

                    // Restore inventory for all items in this suborder
                    await RestoreInventoryAsync(subOrder.OrderItems);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "Order canceled successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error canceling order {OrderId}", orderId);
                return (false, $"Error canceling order: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> CancelSubOrderAsync(int subOrderId, int sellerId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Get the suborder with all related data
                var subOrder = await _context.SubOrders
                    .Include(so => so.OrderItems)
                    .Include(so => so.Order)
                    .FirstOrDefaultAsync(so => so.SuborderId == subOrderId && so.SellerId == sellerId);

                if (subOrder == null)
                    return (false, "Suborder not found or does not belong to this seller");

                // Check if suborder can be canceled (only pending or processing suborders can be canceled)
                if (subOrder.Status != OrderStatus.Pending && subOrder.Status != OrderStatus.Processing)
                    return (false, $"Cannot cancel suborder with status '{subOrder.Status}'");

                // Update suborder status
                subOrder.Status = OrderStatus.Canceled;

                // Restore inventory for all items in this suborder
                await RestoreInventoryAsync(subOrder.OrderItems);

                // Check if all suborders are now canceled, and if so, update the main order status
                var allSubOrders = await _context.SubOrders
                    .Where(so => so.OrderId == subOrder.OrderId)
                    .ToListAsync();

                if (allSubOrders.All(so => so.Status == OrderStatus.Canceled))
                {
                    var order = await _context.Orders.FindAsync(subOrder.OrderId);
                    if (order != null)
                    {
                        order.OrderStatus = OrderStatus.Canceled;
                        order.UpdatedAt = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "Suborder canceled successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error canceling suborder {SubOrderId}", subOrderId);
                return (false, $"Error canceling suborder: {ex.Message}");
            }
        }

        // Helper method to restore inventory when canceling orders
        private async Task RestoreInventoryAsync(ICollection<OrderItem> orderItems)
        {
            foreach (var item in orderItems)
            {
                // If the item has a variant, restore the variant's stock
                if (item.VariantId.HasValue)
                {
                    var variant = await _context.ProductVariants.FindAsync(item.VariantId.Value);
                    if (variant != null)
                    {
                        variant.StockQuantity += item.Quantity;
                        _context.Entry(variant).State = EntityState.Modified;
                    } 
                    if(variant.IsDefault == true)
                    {
                        var product = await _context.Products.FindAsync(item.ProductId);
                        if (product != null)
                        {
                            product.StockQuantity += item.Quantity;
                            _context.Entry(product).State = EntityState.Modified;
                        }
                    }
                }else
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                        _context.Entry(product).State = EntityState.Modified;
                    }
                }

            }
        }
        public async Task<int> GetOrdersCountAsync()
        {
            try
            {
                return await _context.Orders.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order count");
                throw;
            }
        }

        public async Task<int> GetOrdersCountByCustomerIdAsync(int customerId)
        {
            try
            {
                return await _context.Orders
                    .Where(o => o.CustomerId == customerId)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order count for customer {CustomerId}", customerId);
                throw;
            }
        }
        public OrderDto MapOrderToDto(Order order)
        {
            if (order == null)
                return null;

            // Map suborders with their order items
            var suborders = new List<SubOrderDto>();
            foreach (var suborder in order.SubOrders)
            {
                var orderItems = new List<OrderItemDto>();
                foreach (var item in suborder.OrderItems)
                {
                    orderItems.Add(new OrderItemDto()
                    {
                        OrderItemId = item.OrderItemId,
                        SuborderId = item.SuborderId,
                        ProductId = item.ProductId,
                        ProductName = item.Product?.Name, 
                        VariantName = item.Variant.VariantName,
                        Quantity = item.Quantity,
                        PriceAtPurchase = item.PriceAtPurchase,
                        TotalPrice = item.TotalPrice,
                        VariantId = item.VariantId
                    });
                }

                suborders.Add(new SubOrderDto()
                {
                    SuborderId = suborder.SuborderId,
                    OrderId = suborder.OrderId,
                    SellerId = suborder.SellerId,
                    SellerName = suborder.Seller?.User.FirstName + " " + suborder.Seller?.User.LastName, 
                    Subtotal = suborder.Subtotal,
                    Status = suborder.Status,
                    StatusUpdatedAt = suborder.StatusUpdatedAt,
                    TrackingNumber = suborder.TrackingNumber,
                    ShippingProvider = suborder.ShippingProvider,
                    OrderItems = orderItems
                });
            }

            // Format address
            var address = $"{order.Address?.Country}, {order.Address?.State}, {order.Address?.City}";

            return new OrderDto()
            {
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                CustomerName = order.Customer?.User.FirstName + " " + order.Customer?.User.LastName, 
                AddressId = order.AddressId,
                Address = address,
                CouponId = order.CouponId,
                TotalAmount = order.TotalAmount,
                DiscountAmount = order.DiscountAmount,
                ShippingFee = order.ShippingFee,
                TaxAmount = order.TaxAmount,
                FinalAmount = order.FinalAmount,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                AffiliateId = order.AffiliateId,
                AffiliateCode = order.AffiliateCode,
                OrderStatus = order.OrderStatus,
                SubOrders = suborders
            };
        }
        public async Task<OrderDto> GetOrderByIdAsync(int id)
        {
            try
            {
                var order = await _context.Orders
                        .Include(o => o.SubOrders)
                            .ThenInclude(so => so.OrderItems)
                                .ThenInclude(oi => oi.Product) 
                        .Include(o => o.SubOrders)
                            .ThenInclude(so => so.OrderItems)
                                .ThenInclude(oi => oi.Variant) 
                        .Include(o => o.SubOrders)
                            .ThenInclude(so => so.Seller)
                                .ThenInclude(s => s.User)
                        .Include(o => o.Customer)
                            .ThenInclude(c => c.User)
                        .Include(o => o.Address)
                        .FirstOrDefaultAsync(o => o.OrderId == id);

                if (order == null)
                    return null;

                return MapOrderToDto(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order {OrderId}", id);
                throw;
            }
        }

        public async Task<bool> OrderExistsAsync(int id)
        {
            try
            {
                return await _context.Orders.AnyAsync(o => o.OrderId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if order {OrderId} exists", id);
                throw;
            }
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderInputDto orderDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Validate and calculate order
                var (isValid, errorMessage, calculatedOrder) = await ValidateAndCalculateOrderAsync(orderDto);

                if (!isValid)
                    throw new InvalidOperationException(errorMessage);

                // Use the calculated order instead of the input
                orderDto = calculatedOrder;

                // Create the order
                var order = new Order
                {
                    CustomerId = orderDto.CustomerId,
                    AddressId = orderDto.AddressId,
                    CouponId = orderDto.CouponId,
                    TotalAmount = orderDto.TotalAmount,
                    DiscountAmount = orderDto.DiscountAmount,
                    ShippingFee = orderDto.ShippingFee,
                    TaxAmount = orderDto.TaxAmount,
                    FinalAmount = orderDto.FinalAmount,
                    PaymentMethod = orderDto.PaymentMethod,
                    PaymentStatus = PaymentStatus.Pending, // Default status
                    AffiliateId = orderDto.AffiliateId,
                    AffiliateCode = orderDto.AffiliateCode,
                    OrderStatus = OrderStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.Orders.AddAsync(order);
                await _context.SaveChangesAsync();

                // Create sub-orders and order items
                foreach (var subOrderDto in orderDto.SubOrders)
                {
                    var subOrder = new SubOrder
                    {
                        OrderId = order.OrderId,
                        SellerId = subOrderDto.SellerId,
                        Subtotal = subOrderDto.Subtotal,
                        Status = SubOrderStatus.Pending, // Default status
                        StatusUpdatedAt = DateTime.UtcNow
                    };

                    await _context.SubOrders.AddAsync(subOrder);
                    await _context.SaveChangesAsync();

                    // Add order items for this sub-order
                    foreach (var orderItemDto in subOrderDto.OrderItems)
                    {
                        var orderItem = new OrderItem
                        {
                            SuborderId = subOrder.SuborderId,
                            ProductId = orderItemDto.ProductId,
                            Quantity = orderItemDto.Quantity,
                            PriceAtPurchase = orderItemDto.PriceAtPurchase,
                            TotalPrice = orderItemDto.TotalPrice,
                            VariantId = orderItemDto.VariantId
                        };

                        await _context.OrderItems.AddAsync(orderItem);
                    }
                }

                await _context.SaveChangesAsync();

                // Update inventory
                await UpdateInventoryAsync(orderDto);

                await transaction.CommitAsync();

                // Retrieve the newly created order with all details
                var createdOrder = await _context.Orders
                    .Include(o => o.SubOrders)
                        .ThenInclude(so => so.OrderItems)
                    .FirstOrDefaultAsync(o => o.OrderId == order.OrderId);

                return _mapper.Map<OrderDto>(createdOrder);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating order for customer {CustomerId}", orderDto.CustomerId);
                throw;
            }
        }

        public async Task<OrderDto> UpdateOrderAsync(int id, UpdateOrderInputDto orderDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Validate the update
                var (isValid, errorMessage) = await ValidateOrderUpdateAsync(id, orderDto);

                if (!isValid)
                    throw new InvalidOperationException(errorMessage);

                var order = await _context.Orders.FindAsync(id);
                if (order == null)
                    throw new KeyNotFoundException($"Order with ID {id} not found");

                // Update only the provided fields
                if (orderDto.CouponId.HasValue)
                    order.CouponId = orderDto.CouponId;

                if (orderDto.DiscountAmount.HasValue)
                    order.DiscountAmount = orderDto.DiscountAmount;

                if (orderDto.ShippingFee.HasValue)
                    order.ShippingFee = orderDto.ShippingFee;

                if (orderDto.TaxAmount.HasValue)
                    order.TaxAmount = orderDto.TaxAmount;

                if (orderDto.FinalAmount.HasValue)
                    order.FinalAmount = orderDto.FinalAmount.Value;

                if (!string.IsNullOrEmpty(orderDto.PaymentStatus))
                    order.PaymentStatus = orderDto.PaymentStatus;

                // Add this block to handle OrderStatus updates
                if (!string.IsNullOrEmpty(orderDto.OrderStatus))
                    order.OrderStatus = orderDto.OrderStatus;

                order.UpdatedAt = DateTime.UtcNow;

                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // Get the updated order with all details
                var updatedOrder = await _context.Orders
                    .Include(o => o.SubOrders)
                        .ThenInclude(so => so.OrderItems)
                    .FirstOrDefaultAsync(o => o.OrderId == id);

                return _mapper.Map<OrderDto>(updatedOrder);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating order {OrderId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // First, delete all order items in all sub-orders
                var subOrders = await _context.SubOrders
                    .Where(so => so.OrderId == id)
                    .ToListAsync();

                foreach (var subOrder in subOrders)
                {
                    var orderItems = await _context.OrderItems
                        .Where(oi => oi.SuborderId == subOrder.SuborderId)
                        .ToListAsync();

                    _context.OrderItems.RemoveRange(orderItems);
                }

                await _context.SaveChangesAsync();

                // Next, delete all sub-orders
                _context.SubOrders.RemoveRange(subOrders);
                await _context.SaveChangesAsync();

                // Finally, delete the order
                var order = await _context.Orders.FindAsync(id);
                if (order == null)
                    return false;

                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting order {OrderId}", id);
                throw;
            }
        }

        public async Task<OrderDto> UpdateOrderPaymentStatusAsync(int id, string paymentStatus)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null)
                    throw new KeyNotFoundException($"Order with ID {id} not found");

                order.PaymentStatus = paymentStatus;
                order.UpdatedAt = DateTime.UtcNow;

                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                // Get the updated order with all details
                var updatedOrder = await _context.Orders
                    .Include(o => o.SubOrders)
                        .ThenInclude(so => so.OrderItems)
                    .FirstOrDefaultAsync(o => o.OrderId == id);

                return _mapper.Map<OrderDto>(updatedOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment status for order {OrderId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByPaymentStatusAsync(string paymentStatus, PaginationDto pagination)
        {
            try
            {
                var orders = await _context.Orders
                    .Where(o => o.PaymentStatus == paymentStatus)
                    .Include(o => o.SubOrders)
                        .ThenInclude(so => so.OrderItems)
                    .OrderByDescending(o => o.CreatedAt)
                    .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                    .Take(pagination.PageSize)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<OrderDto>>(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders with payment status {PaymentStatus}", paymentStatus);
                throw;
            }
        }

        public async Task<int> GetOrdersCountByPaymentStatusAsync(string paymentStatus)
        {
            try
            {
                return await _context.Orders
                    .Where(o => o.PaymentStatus == paymentStatus)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting count of orders with payment status {PaymentStatus}", paymentStatus);
                throw;
            }
        }

        public async Task<SubOrderDto> GetSubOrderByIdAsync(int id)
        {
            try
            {
                var subOrder = await _context.SubOrders
                    .Include(so => so.OrderItems)
                    .FirstOrDefaultAsync(so => so.SuborderId == id);

                if (subOrder == null)
                    return null;

                return _mapper.Map<SubOrderDto>(subOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sub-order {SubOrderId}", id);
                throw;
            }
        }

        public async Task<SubOrderDto> UpdateSubOrderStatusAsync(int id, UpdateSubOrderInputDto subOrderDto)
        {
            try
            {
                var subOrder = await _context.SubOrders.FindAsync(id);
                if (subOrder == null)
                    throw new KeyNotFoundException($"SubOrder with ID {id} not found");

                // Update only the provided fields
                if (!string.IsNullOrEmpty(subOrderDto.Status))
                {
                    subOrder.Status = subOrderDto.Status;
                    subOrder.StatusUpdatedAt = DateTime.UtcNow;
                }

                if (!string.IsNullOrEmpty(subOrderDto.TrackingNumber))
                    subOrder.TrackingNumber = subOrderDto.TrackingNumber;

                if (!string.IsNullOrEmpty(subOrderDto.ShippingProvider))
                    subOrder.ShippingProvider = subOrderDto.ShippingProvider;

                _context.SubOrders.Update(subOrder);
                await _context.SaveChangesAsync();

                // Get the updated sub-order with order items
                var updatedSubOrder = await _context.SubOrders
                    .Include(so => so.OrderItems)
                    .FirstOrDefaultAsync(so => so.SuborderId == id);

                return _mapper.Map<SubOrderDto>(updatedSubOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for sub-order {SubOrderId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<SubOrderDto>> GetSubOrdersBySellerIdAsync(int sellerId, PaginationDto pagination)
        {
            try
            {
                var subOrders = await _context.SubOrders
                    .Where(so => so.SellerId == sellerId)
                    .Include(so => so.OrderItems)
                    .OrderByDescending(so => so.StatusUpdatedAt)
                    .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                    .Take(pagination.PageSize)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<SubOrderDto>>(subOrders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sub-orders for seller {SellerId}", sellerId);
                throw;
            }
        }

        public async Task<int> GetSubOrdersCountBySellerIdAsync(int sellerId)
        {
            try
            {
                return await _context.SubOrders
                    .Where(so => so.SellerId == sellerId)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting count of sub-orders for seller {SellerId}", sellerId);
                throw;
            }
        }

        public async Task<IEnumerable<SubOrderDto>> GetSubOrdersByStatusAsync(string status, PaginationDto pagination)
        {
            try
            {
                var subOrders = await _context.SubOrders
                    .Where(so => so.Status == status)
                    .Include(so => so.OrderItems)
                    .OrderByDescending(so => so.StatusUpdatedAt)
                    .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                    .Take(pagination.PageSize)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<SubOrderDto>>(subOrders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sub-orders with status {Status}", status);
                throw;
            }
        }

        public async Task<int> GetSubOrdersCountByStatusAsync(string status)
        {
            try
            {
                return await _context.SubOrders
                    .Where(so => so.Status == status)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting count of sub-orders with status {Status}", status);
                throw;
            }
        }

        // Add these helper methods to your OrderRepository class

        /// <summary>
        /// Validates and calculates order totals before creating or updating an order
        /// </summary>
        private async Task<(bool IsValid, string ErrorMessage, CreateOrderInputDto CalculatedOrder)> ValidateAndCalculateOrderAsync(CreateOrderInputDto orderDto)
        {
            try
            {
                // Create a copy of the order for calculations
                var calculatedOrder = new CreateOrderInputDto
                {
                    CustomerId = orderDto.CustomerId,
                    AddressId = orderDto.AddressId,
                    CouponId = orderDto.CouponId,
                    PaymentMethod = orderDto.PaymentMethod,
                    AffiliateId = orderDto.AffiliateId,
                    AffiliateCode = orderDto.AffiliateCode,
                    OrderItems = orderDto.OrderItems,
                    SubOrders = new List<CreateSubOrderInputDto>()
                };

                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == orderDto.CustomerId);
                if (customer == null)
                    return (false, $"Customer with ID {orderDto.CustomerId} not found", null);

                var address = await _context.Addresses.FirstOrDefaultAsync(a => a.AddressId == orderDto.AddressId && a.UserId == customer.UserId);
                if (address == null)
                    return (false, $"Address with ID {orderDto.AddressId} not found or does not belong to the customer", null);

                // Group order items by seller
                if (calculatedOrder.OrderItems != null && calculatedOrder.OrderItems.Count > 0)
                {
                    // Dictionary to store items by seller ID
                    var itemsBySeller = new Dictionary<int, List<CreateOrderItemInputDto>>();

                    // Process each order item
                    foreach (var orderItem in calculatedOrder.OrderItems)
                    {
                        // Get product details including seller ID
                        var product = await _context.Products
                            .FirstOrDefaultAsync(p => p.ProductId == orderItem.ProductId && p.IsAvailable == true && p.ApprovalStatus != ProductApprovalStatus.Deleted && p.ApprovalStatus != ProductApprovalStatus.Pending && p.ApprovalStatus != ProductApprovalStatus.PendingReview);

                        if (product == null)
                            return (false, $"Product with ID {orderItem.ProductId} not found or is not available", null);

                        // Validate quantity for the main product
                        if ((!orderItem.VariantId.HasValue || orderItem.VariantId == 0) && product.StockQuantity < orderItem.Quantity)
                            return (false, $"Product with ID {orderItem.ProductId} does not have enough stock. Available: {product.StockQuantity}", null);

                        // Validate variant if specified
                        if (orderItem.VariantId.HasValue)
                        {
                            var variant = await _context.ProductVariants
                                .FirstOrDefaultAsync(v => v.VariantId == orderItem.VariantId &&
                                                       v.ProductId == orderItem.ProductId &&
                                                       v.IsAvailable == true);

                            if (variant == null)
                                return (false, $"Variant with ID {orderItem.VariantId} not found or is not available", null);

                            if (variant.StockQuantity < orderItem.Quantity)
                                return (false, $"Variant with ID {orderItem.VariantId} does not have enough stock. Available: {variant.StockQuantity}", null);
                        }

                        // Get seller ID
                        int sellerId = product.SellerId;

                        // Verify seller exists and is verified
                        var seller = await _context.Sellers.FirstOrDefaultAsync(s => s.SellerId == sellerId && s.IsVerified == true);
                        if (seller == null)
                            return (false, $"Seller with ID {sellerId} not found or is not verified", null);

                        // Add item to the appropriate seller group
                        if (!itemsBySeller.ContainsKey(sellerId))
                        {
                            itemsBySeller[sellerId] = new List<CreateOrderItemInputDto>();
                        }

                        itemsBySeller[sellerId].Add(orderItem);
                    }

                    // Create sub-orders from the grouped items
                    decimal orderSubtotal = 0;

                    foreach (var sellerGroup in itemsBySeller)
                    {
                        int sellerId = sellerGroup.Key;
                        var items = sellerGroup.Value;

                        var calculatedSubOrder = new CreateSubOrderInputDto
                        {
                            SellerId = sellerId,
                            OrderItems = new List<CreateOrderItemInputDto>(),
                        };

                        decimal subOrderTotal = 0;

                        // Process each item in this seller group
                        foreach (var itemDto in items)
                        {
                            var product = await _context.Products.FindAsync(itemDto.ProductId);

                            // Calculate item price based on product or variant
                            decimal itemPrice;

                            if (itemDto.VariantId.HasValue)
                            {
                                var variant = await _context.ProductVariants.FindAsync(itemDto.VariantId.Value);

                                // Calculate price with discount if applicable
                                itemPrice = variant.Price;
                                if (variant.DiscountPercentage.HasValue && variant.DiscountPercentage > 0)
                                {
                                    itemPrice = itemPrice - (itemPrice * variant.DiscountPercentage.Value / 100);
                                }
                            }
                            else
                            {
                                // Calculate price with discount if applicable
                                itemPrice = product.BasePrice;
                                if (product.DiscountPercentage.HasValue && product.DiscountPercentage > 0)
                                {
                                    itemPrice = itemPrice - (itemPrice * product.DiscountPercentage.Value / 100);
                                }
                            }

                            // Round to 2 decimal places
                            itemPrice = Math.Round(itemPrice, 2);

                            // Calculate total for this item
                            decimal itemTotal = itemPrice * itemDto.Quantity;

                            // Add to sub-order total
                            subOrderTotal += itemTotal;

                            // Create calculated order item
                            var calculatedItem = new CreateOrderItemInputDto
                            {
                                ProductId = itemDto.ProductId,
                                Quantity = itemDto.Quantity,
                                PriceAtPurchase = itemPrice,
                                TotalPrice = itemTotal,
                                VariantId = itemDto.VariantId
                            };

                            calculatedSubOrder.OrderItems.Add(calculatedItem);
                        }

                        // Set the calculated subtotal
                        calculatedSubOrder.Subtotal = Math.Round(subOrderTotal, 2);
                        orderSubtotal += subOrderTotal;

                        calculatedOrder.SubOrders.Add(calculatedSubOrder);
                    }

                    // Step 3: Calculate discount from coupon if provided
                    decimal discountAmount = 0;

                    if (orderDto.CouponId.HasValue)
                    {
                        var coupon = await _context.Coupons
                            .FirstOrDefaultAsync(c => c.CouponId == orderDto.CouponId && c.IsActive == true);

                        if (coupon == null)
                            return (false, $"Coupon with ID {orderDto.CouponId} not found or is not active", null);

                        // Verify coupon date validity
                        var currentDate = DateTime.UtcNow;
                        if (currentDate < coupon.StartDate || currentDate > coupon.EndDate)
                            return (false, "Coupon is not valid at this time", null);

                        // Verify minimum purchase
                        if (coupon.MinimumPurchase.HasValue && orderSubtotal < coupon.MinimumPurchase.Value)
                            return (false, $"Order subtotal does not meet the minimum purchase requirement of {coupon.MinimumPurchase.Value:C} for this coupon", null);

                        // Calculate discount
                        if (coupon.DiscountType == CouponType.Fixed)
                        {
                            discountAmount = coupon.DiscountAmount;
                        }
                        else if (coupon.DiscountType == CouponType.Percentage)
                        {
                            discountAmount = orderSubtotal * (coupon.DiscountAmount / 100);
                        }

                        // Cap discount at the order subtotal
                        discountAmount = Math.Min(discountAmount, orderSubtotal);
                        discountAmount = Math.Round(discountAmount, 2);
                    }

                    // Step 4: Calculate tax and shipping
                    decimal taxRate = 0.05m;
                    decimal taxAmount = Math.Round(orderSubtotal * taxRate, 2);
                    decimal shippingFee = 10.00m;

                    // Reduce or eliminate shipping for larger orders
                    if (orderSubtotal > 100)
                        shippingFee = 5.00m;

                    if (orderSubtotal > 200)
                        shippingFee = 0.00m;

                    // Step 5: Calculate final amount
                    decimal finalAmount = orderSubtotal - discountAmount + taxAmount + shippingFee;
                    finalAmount = Math.Round(finalAmount, 2);

                    // Set all calculated values to the order
                    calculatedOrder.TotalAmount = orderSubtotal;
                    calculatedOrder.DiscountAmount = discountAmount;
                    calculatedOrder.TaxAmount = taxAmount;
                    calculatedOrder.ShippingFee = shippingFee;
                    calculatedOrder.FinalAmount = finalAmount;
                }
                else
                {
                    return (false, "No order items provided", null);
                }

                return (true, string.Empty, calculatedOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating and calculating order");
                return (false, $"Error validating order: {ex.Message}", null);
            }
        }
        /// <summary>
        /// Updates inventory quantities after a successful order
        /// </summary>
        private async Task UpdateInventoryAsync(CreateOrderInputDto orderDto)
        {
            foreach (var subOrder in orderDto.SubOrders)
            {
                foreach (var item in subOrder.OrderItems)
                {
                    if (item.VariantId.HasValue)
                    {
                        // Update variant quantity
                        var variant = await _context.ProductVariants.FindAsync(item.VariantId.Value);
                        if (variant != null)
                        {
                            variant.StockQuantity -= item.Quantity;
                            _context.Entry(variant).State = EntityState.Modified;
                        } 
                        if(variant.IsDefault == true)
                        {
                            
                            // Always update product quantity
                            var product = await _context.Products.FindAsync(item.ProductId);
                            if (product != null)
                            {
                                product.StockQuantity -= item.Quantity;
                                _context.Entry(product).State = EntityState.Modified;
                            }
                        }
                    }
                    else
                    {
                        // Always update product quantity
                        var product = await _context.Products.FindAsync(item.ProductId);
                        if (product != null)
                        {
                            product.StockQuantity -= item.Quantity;
                            _context.Entry(product).State = EntityState.Modified;
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Validates an update to an existing order
        /// </summary>
        private async Task<(bool IsValid, string ErrorMessage)> ValidateOrderUpdateAsync(int orderId, UpdateOrderInputDto updateDto)
        {
            // Get the existing order
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return (false, $"Order with ID {orderId} not found");

            // If changing payment status, validate the status value
            if (!string.IsNullOrEmpty(updateDto.PaymentStatus))
            {
                var validStatuses = new[] { "pending", "paid", "failed", "refunded", "partially_refunded" };
                if (!validStatuses.Contains(updateDto.PaymentStatus.ToLower()))
                    return (false, $"Invalid payment status: {updateDto.PaymentStatus}. Valid values are: {string.Join(", ", validStatuses)}");
            }

            // If updating coupon, validate the coupon
            if (updateDto.CouponId.HasValue)
            {
                var coupon = await _context.Coupons
                    .FirstOrDefaultAsync(c => c.CouponId == updateDto.CouponId && c.IsActive == true);

                if (coupon == null)
                    return (false, $"Coupon with ID {updateDto.CouponId} not found or is not active");

                // Verify coupon date validity
                var currentDate = DateTime.UtcNow;
                if (currentDate < coupon.StartDate || currentDate > coupon.EndDate)
                    return (false, "Coupon is not valid at this time");

                // Check minimum purchase requirement
                // For this, we would need the current order total, which might be affected by other changes in the update
                // For simplicity, we'll skip this check in the update validation
            }

            return (true, string.Empty);
        }
    }
}