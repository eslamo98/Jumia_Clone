using Jumia_Clone.Models.DTOs.OrderDTOs;
using Jumia_Clone.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jumia_Clone.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;

        public OrdersController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
        {
            var order = await _orderRepository.CreateOrderAsync(dto);
            return Ok(order);
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderDetails(int orderId)
        {
            var order = await _orderRepository.GetOrderDetailsAsync(orderId);
            if (order == null) return NotFound();
            return Ok(order);
        }

        [HttpGet("history/{customerId}")]
        public async Task<IActionResult> GetOrderHistory(int customerId)
        {
            var orders = await _orderRepository.GetOrderHistoryAsync(customerId);
            return Ok(orders);
        }

        [HttpDelete("{orderId}")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var result = await _orderRepository.CancelOrderAsync(orderId);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
