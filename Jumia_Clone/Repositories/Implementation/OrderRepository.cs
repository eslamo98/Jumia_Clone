using AutoMapper;
using Jumia_Clone.Data;
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

        public async Task<OrderDto> GetOrderByIdAsync(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.SubOrders)
                        .ThenInclude(so => so.OrderItems)
                    .FirstOrDefaultAsync(o => o.OrderId == id);

                if (order == null)
                    return null;

                return _mapper.Map<OrderDto>(order);
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
                    PaymentStatus = "pending", // Default status
                    AffiliateId = orderDto.AffiliateId,
                    AffiliateCode = orderDto.AffiliateCode,
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
                        Status = "pending", // Default status
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
                    SubOrders = new List<CreateSubOrderInputDto>()
                };

                // Step 1: Validate customer and address
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == orderDto.CustomerId);
                if (customer == null)
                    return (false, $"Customer with ID {orderDto.CustomerId} not found", null);

                var address = await _context.Addresses.FirstOrDefaultAsync(a => a.AddressId == orderDto.AddressId && a.UserId == customer.UserId);
                if (address == null)
                    return (false, $"Address with ID {orderDto.AddressId} not found or does not belong to the customer", null);

                // Step 2: Process each sub-order and calculate subtotals
                decimal orderSubtotal = 0;

                foreach (var subOrderDto in orderDto.SubOrders)
                {
                    // Verify seller exists
                    var seller = await _context.Sellers.FirstOrDefaultAsync(s => s.SellerId == subOrderDto.SellerId && s.IsVerified == true);
                    if (seller == null)
                        return (false, $"Seller with ID {subOrderDto.SellerId} not found or is not verified", null);

                    var calculatedSubOrder = new CreateSubOrderInputDto
                    {
                        SellerId = subOrderDto.SellerId,
                        OrderItems = new List<CreateOrderItemInputDto>(),
                    };

                    decimal subOrderTotal = 0;

                    // Process each item in this sub-order
                    foreach (var itemDto in subOrderDto.OrderItems)
                    {
                        // Validate product
                        var product = await _context.Products
                            .FirstOrDefaultAsync(p => p.ProductId == itemDto.ProductId && p.IsAvailable == true);

                        if (product == null)
                            return (false, $"Product with ID {itemDto.ProductId} not found or is not available", null);

                        // Validate quantity
                        if (product.StockQuantity < itemDto.Quantity)
                            return (false, $"Product with ID {itemDto.ProductId} does not have enough stock. Available: {product.StockQuantity}", null);

                        // Calculate item price based on product or variant
                        decimal itemPrice;

                        if (itemDto.VariantId.HasValue)
                        {
                            var variant = await _context.ProductVariants
                                .FirstOrDefaultAsync(v => v.VariantId == itemDto.VariantId &&
                                                       v.ProductId == itemDto.ProductId &&
                                                       v.IsAvailable == true);

                            if (variant == null)
                                return (false, $"Variant with ID {itemDto.VariantId} not found or is not available", null);

                            if (variant.StockQuantity < itemDto.Quantity)
                                return (false, $"Variant with ID {itemDto.VariantId} does not have enough stock. Available: {variant.StockQuantity}", null);

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
                    if (coupon.DiscountType == "Fixed")
                    {
                        discountAmount = coupon.DiscountAmount;
                    }
                    else if (coupon.DiscountType == "Percentage")
                    {
                        discountAmount = orderSubtotal * (coupon.DiscountAmount / 100);
                    }

                    // Cap discount at the order subtotal
                    discountAmount = Math.Min(discountAmount, orderSubtotal);
                    discountAmount = Math.Round(discountAmount, 2);
                }

                // Step 4: Calculate tax and shipping
                // Note: This is a simplified calculation. In a real system, tax and shipping
                // would depend on customer location, product categories, shipping methods, etc.
                decimal taxRate = 0.05m; // 5% tax - you might want to make this configurable
                decimal taxAmount = Math.Round(orderSubtotal * taxRate, 2);

                // Base shipping fee (you might want to calculate this based on items, weight, distance, etc.)
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
                    }

                    // Always update product quantity
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity -= item.Quantity;
                        _context.Entry(product).State = EntityState.Modified;
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