using Jumia_Clone.Models.DTOs.OrderDTOs;
using Jumia_Clone.Models.Entities;

namespace Jumia_Clone.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<OrderDetailsDto> CreateOrderAsync(CreateOrderDto dto);
        Task<OrderDetailsDto> GetOrderDetailsAsync(int orderId);
        Task<IEnumerable<OrderDetailsDto>> GetOrderHistoryAsync(int customerId);
        Task<bool> CancelOrderAsync(int orderId);
    }
}
