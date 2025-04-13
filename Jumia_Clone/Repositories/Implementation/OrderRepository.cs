using Jumia_Clone.Data;
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
        public OrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OrderDetailsDto> CreateOrderAsync(CreateOrderDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = new Order
                {
                    CustomerId = dto.CustomerId,
                    AddressId = dto.AddressId,
                    CouponId = dto.CouponId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    PaymentMethod = dto.PaymentMethod,
                    PaymentStatus = "Pending",
                    SubOrders = new List<SubOrder>()
                };

                // Load all products to avoid querying for each item
                var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
                var products = await _context.Products.Where(p => productIds.Contains(p.ProductId)).ToListAsync();

                // Group by seller
                var sellerGroups = dto.Items.GroupBy(i => products.First(p => p.ProductId == i.ProductId).SellerId);

                foreach (var sellerGroup in sellerGroups)
                {
                    var subOrder = new SubOrder
                    {
                        SellerId = sellerGroup.Key,
                        OrderItems = new List<OrderItem>()
                    };

                    foreach (var item in sellerGroup)
                    {
                        var product = products.FirstOrDefault(p => p.ProductId == item.ProductId);
                        if (product == null)
                        {
                            throw new ArgumentException($"Product with ID {item.ProductId} not found.");
                        }

                        if (item.Quantity <= 0 || item.PriceAtPurchase <= 0)
                        {
                            throw new ArgumentException($"Invalid quantity or price for product ID {item.ProductId}.");
                        }

                        var total = item.Quantity * item.PriceAtPurchase;
                        subOrder.OrderItems.Add(new OrderItem
                        {
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            PriceAtPurchase = item.PriceAtPurchase,
                            TotalPrice = total,
                            VariantId = item.VariantId
                        });

                        order.TotalAmount += total;
                    }

                    order.SubOrders.Add(subOrder);
                }

                order.FinalAmount = order.TotalAmount;

                // Save to database
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // Map to DTO before returning
                return new OrderDetailsDto
                {
                    OrderId = order.OrderId,
                    TotalAmount = order.TotalAmount,
                    FinalAmount = order.FinalAmount,
                    PaymentMethod = order.PaymentMethod,
                    PaymentStatus = order.PaymentStatus,
                    SubOrders = order.SubOrders.Select(subOrder => new SubOrderDto
                    {
                        SellerId = subOrder.SellerId,
                        OrderItems = subOrder.OrderItems.Select(oi => new OrderItemDto
                        {
                            ProductId = oi.ProductId,
                            Quantity = oi.Quantity,
                            PriceAtPurchase = oi.PriceAtPurchase,
                            VariantId = oi.VariantId
                        }).ToList()
                    }).ToList()
                };
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;  // Re-throw the exception
            }
        }

        // Updated GetOrderDetailsAsync to use SubOrderDto
        public async Task<OrderDetailsDto> GetOrderDetailsAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.SubOrders).ThenInclude(s => s.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null) return null;

            // Changed this part to map SubOrder to SubOrderDto
            var subOrders = order.SubOrders.Select(subOrder => new SubOrderDto
            {
                SellerId = subOrder.SellerId,
                OrderItems = subOrder.OrderItems.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    PriceAtPurchase = oi.PriceAtPurchase,
                    VariantId = oi.VariantId
                }).ToList()
            }).ToList();

            return new OrderDetailsDto
            {
                OrderId = order.OrderId,
                TotalAmount = order.TotalAmount,
                FinalAmount = order.FinalAmount,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                SubOrders = subOrders  // Updated to return SubOrderDto
            };
        }

        // Updated GetOrderHistoryAsync to use SubOrderDto
        public async Task<IEnumerable<OrderDetailsDto>> GetOrderHistoryAsync(int customerId)
        {
            var orders = await _context.Orders
                .Where(o => o.CustomerId == customerId)
                .Include(o => o.SubOrders).ThenInclude(s => s.OrderItems).ThenInclude(oi => oi.Product)
                .ToListAsync();

            // Changed this part to map SubOrder to SubOrderDto
            return orders.Select(order => new OrderDetailsDto
            {
                OrderId = order.OrderId,
                TotalAmount = order.TotalAmount,
                FinalAmount = order.FinalAmount,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                SubOrders = order.SubOrders.Select(subOrder => new SubOrderDto
                {
                    SellerId = subOrder.SellerId,
                    OrderItems = subOrder.OrderItems.Select(oi => new OrderItemDto
                    {
                        ProductId = oi.ProductId,
                        Quantity = oi.Quantity,
                        PriceAtPurchase = oi.PriceAtPurchase,
                        VariantId = oi.VariantId
                    }).ToList()
                }).ToList() // Updated to return SubOrderDto
            });
        }

        public async Task<bool> CancelOrderAsync(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;
            // You can add more business logic here (check status etc.)
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}