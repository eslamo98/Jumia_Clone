using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Models.DTOs.OrderDTOs;
using Jumia_Clone.Models.Entities;

namespace Jumia_Clone.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        // Get all orders with pagination
        Task<IEnumerable<OrderDto>> GetOrdersAsync(PaginationDto pagination);

        // Get orders by customer ID with pagination
        Task<IEnumerable<OrderDto>> GetOrdersByCustomerIdAsync(int customerId, PaginationDto pagination);

        // Get total count of orders
        Task<int> GetOrdersCountAsync();

        // Get total count of orders for a specific customer
        Task<int> GetOrdersCountByCustomerIdAsync(int customerId);

        // Get a specific order by ID
        Task<OrderDto> GetOrderByIdAsync(int id);

        // Check if an order exists
        Task<bool> OrderExistsAsync(int id);

        // Create a new order
        Task<OrderDto> CreateOrderAsync(CreateOrderInputDto orderDto);

        // Update an order
        Task<OrderDto> UpdateOrderAsync(int id, UpdateOrderInputDto orderDto);

        // Delete an order
        Task<bool> DeleteOrderAsync(int id);

        // Update order payment status
        Task<OrderDto> UpdateOrderPaymentStatusAsync(int id, string paymentStatus);

        // Get orders by payment status with pagination
        Task<IEnumerable<OrderDto>> GetOrdersByPaymentStatusAsync(string paymentStatus, PaginationDto pagination);

        // Get count of orders by payment status
        Task<int> GetOrdersCountByPaymentStatusAsync(string paymentStatus);

        // Get SubOrder by ID
        Task<SubOrderDto> GetSubOrderByIdAsync(int id);

        // Update SubOrder status
        Task<SubOrderDto> UpdateSubOrderStatusAsync(int id, UpdateSubOrderInputDto subOrderDto);

        // Get SubOrders by seller ID with pagination
        Task<IEnumerable<SubOrderDto>> GetSubOrdersBySellerIdAsync(int sellerId, PaginationDto pagination);

        // Get count of SubOrders by seller ID
        Task<int> GetSubOrdersCountBySellerIdAsync(int sellerId);

        // Get SubOrders by status with pagination
        Task<IEnumerable<SubOrderDto>> GetSubOrdersByStatusAsync(string status, PaginationDto pagination);

        // Get count of SubOrders by status
        Task<int> GetSubOrdersCountByStatusAsync(string status);
    }
}
