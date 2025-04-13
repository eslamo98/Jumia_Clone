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
            try
            {
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

                // Get the updated order with all details
                var updatedOrder = await _context.Orders
                    .Include(o => o.SubOrders)
                        .ThenInclude(so => so.OrderItems)
                    .FirstOrDefaultAsync(o => o.OrderId == id);

                return _mapper.Map<OrderDto>(updatedOrder);
            }
            catch (Exception ex)
            {
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
    }
}