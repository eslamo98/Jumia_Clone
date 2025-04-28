using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Models.DTOs.OrderDTOs;
using Jumia_Clone.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Jumia_Clone.Models.Constants;

namespace Jumia_Clone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        //private readonly IMemoryCache _cache;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(
            IOrderRepository orderRepository,
            IMapper mapper,
            ////IMemoryCache cache,
            ILogger<OrdersController> logger)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            //_cache = cache;
            _logger = logger;
        }

        // GET: api/orders
        [HttpGet]
        [EnableRateLimiting("standard")]
        [ResponseCache(Duration = 60)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] PaginationDto pagination)
        {
            try
            {
                // Create a cache key based on parameters
                //var cacheKey = $"orders_all_{pagination.PageNumber}_{pagination.PageSize}";

                //// Try to get from cache first
                //if (_cache.TryGetValue(cacheKey, out ApiResponse<object> cachedResult))
                //{
                //    return Ok(cachedResult);
                //}

                // Make sure pagination is valid
                if (pagination.PageNumber < 1)
                    pagination.PageNumber = 1;

                if (pagination.PageSize < 1)
                    pagination.PageSize = 10;

                var orders = await _orderRepository.GetOrdersAsync(pagination);
                var totalCount = await _orderRepository.GetOrdersCountAsync();

                var response = new ApiResponse<object>
                {
                    Message = "Successfully retrieved orders",
                    Data = new
                    {
                        Items = orders,
                        TotalCount = totalCount,
                        PageNumber = pagination.PageNumber,
                        PageSize = pagination.PageSize,
                        TotalPages = (int)Math.Ceiling(totalCount / (double)pagination.PageSize),
                        HasPreviousPage = pagination.PageNumber > 1,
                        HasNextPage = pagination.PageNumber < (int)Math.Ceiling(totalCount / (double)pagination.PageSize)
                    },
                    Success = true
                };

              

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving orders");
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving orders",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }
        // POST: api/orders/{id}/cancel
        [HttpPost("{id}/cancel")]
        [Authorize(Roles = $"{UserRoles.Customer},{UserRoles.Admin}")]
        [EnableRateLimiting("standard")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            try
            {
                var result = await _orderRepository.CancelOrderAsync(id, 0);

                if (!result.Success)
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        Message = "Failed to cancel order",
                        ErrorMessages = new string[] { result.Message }
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = result.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling order {OrderId}", id);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while canceling the order",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // POST: api/orders/suborders/{id}/cancel
        [HttpPost("suborders/{id}/cancel")]
        [Authorize(Roles = "Seller")]
        [EnableRateLimiting("standard")]
        public async Task<IActionResult> CancelSubOrder(int id, [FromQuery] int sellerId)
        {
            try
            {
                
                if (string.IsNullOrEmpty(sellerId.ToString()) || sellerId == 0)
                {
                    return Unauthorized(new ApiErrorResponse
                    {
                        Message = "Invalid seller credentials",
                        ErrorMessages = new string[] { "You must be logged in as a seller to cancel a suborder" }
                    });
                }

                var result = await _orderRepository.CancelSubOrderAsync(id, sellerId);

                if (!result.Success)
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        Message = "Failed to cancel suborder",
                        ErrorMessages = new string[] { result.Message }
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = result.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling suborder {SubOrderId}", id);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while canceling the suborder",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }
        // GET: api/orders/customer/{customerId}
        [HttpGet("customer/{customerId}")]
        [EnableRateLimiting("standard")]
        [ResponseCache(Duration = 60)]
        public async Task<IActionResult> GetByCustomerId(int customerId, [FromQuery] PaginationDto pagination)
        {
            try
            {
                // Create a cache key based on parameters
                //var cacheKey = $"orders_customer_{customerId}_{pagination.PageNumber}_{pagination.PageSize}";

                //// Try to get from cache first
                //if (_cache.TryGetValue(cacheKey, out ApiResponse<object> cachedResult))
                //{
                //    return Ok(cachedResult);
                //}

                // Make sure pagination is valid
                if (pagination.PageNumber < 1)
                    pagination.PageNumber = 1;

                if (pagination.PageSize < 1)
                    pagination.PageSize = 10;

                var orders = await _orderRepository.GetOrdersByCustomerIdAsync(customerId, pagination);
                var totalCount = await _orderRepository.GetOrdersCountByCustomerIdAsync(customerId);

                var response = new ApiResponse<object>
                {
                    Message = "Successfully retrieved orders for customer",
                    Data = new
                    {
                        Items = orders,
                        TotalCount = totalCount,
                        PageNumber = pagination.PageNumber,
                        PageSize = pagination.PageSize,
                        TotalPages = (int)Math.Ceiling(totalCount / (double)pagination.PageSize),
                        HasPreviousPage = pagination.PageNumber > 1,
                        HasNextPage = pagination.PageNumber < (int)Math.Ceiling(totalCount / (double)pagination.PageSize)
                    },
                    Success = true
                };

                // Cache the result
                //var cacheEntryOptions = new MemoryCacheEntryOptions()
                //    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                //    .SetSlidingExpiration(TimeSpan.FromMinutes(1));

                //_cache.Set(cacheKey, response, cacheEntryOptions);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving orders for customer {CustomerId}", customerId);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving customer orders",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }
        // PUT: api/orders/{id}/status
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("standard")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto statusDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        Message = "Invalid input",
                        ErrorMessages = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToArray()
                    });
                }

                // Validate the status value
                if (!OrderStatus.AllowedValues.Contains(statusDto.Status))
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        Message = "Invalid order status",
                        ErrorMessages = new[] { $"Valid statuses are: {string.Join(", ", OrderStatus.AllowedValues)}" }
                    });
                }

                // Check if order exists
                if (!await _orderRepository.OrderExistsAsync(id))
                {
                    return NotFound(new ApiErrorResponse
                    {
                        Message = "Order not found",
                        ErrorMessages = new[] { $"Order with ID {id} does not exist" }
                    });
                }

                // Update the order status
                var updateDto = new UpdateOrderInputDto
                {
                    OrderStatus = statusDto.Status
                };

                var updatedOrder = await _orderRepository.UpdateOrderAsync(id, updateDto);

                return Ok(new ApiResponse<OrderDto>
                {
                    Success = true,
                    Message = "Order status updated successfully",
                    Data = updatedOrder
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status for order {OrderId}", id);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while updating the order status",
                    ErrorMessages = new[] { ex.Message }
                });
            }
        }

        // PUT: api/orders/suborders/{id}/status
        [HttpPut("suborders/{id}/status")]
        [Authorize(Roles = "Admin,Seller")]
        [EnableRateLimiting("standard")]
        public async Task<IActionResult> UpdateSubOrderStatus(int id, [FromBody] UpdateSubOrderStatusDto statusDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        Message = "Invalid input",
                        ErrorMessages = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToArray()
                    });
                }

                // Validate the status value
                if (!SubOrderStatus.AllowedValues.Contains(statusDto.Status))
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        Message = "Invalid suborder status",
                        ErrorMessages = new[] { $"Valid statuses are: {string.Join(", ", SubOrderStatus.AllowedValues)}" }
                    });
                }

                // For sellers, verify they own this suborder
                if (User.IsInRole("Seller"))
                {
                    var sellerId = User.FindFirst("SellerId")?.Value;
                    if (string.IsNullOrEmpty(sellerId) || !int.TryParse(sellerId, out int sellerIdInt))
                    {
                        return Unauthorized(new ApiErrorResponse
                        {
                            Message = "Invalid seller credentials",
                            ErrorMessages = new[] { "You must be logged in as a seller to update a suborder" }
                        });
                    }

                    var subOrder = await _orderRepository.GetSubOrderByIdAsync(id);
                    if (subOrder == null)
                    {
                        return NotFound(new ApiErrorResponse
                        {
                            Message = "Suborder not found",
                            ErrorMessages = new[] { $"Suborder with ID {id} does not exist" }
                        });
                    }

                    if (subOrder.SellerId != sellerIdInt)
                    {
                        return Forbid();
                    }
                }

                // Update the suborder status
                var updateDto = new UpdateSubOrderInputDto
                {
                    Status = statusDto.Status
                };

                var updatedSubOrder = await _orderRepository.UpdateSubOrderStatusAsync(id, updateDto);

                return Ok(new ApiResponse<SubOrderDto>
                {
                    Success = true,
                    Message = "Suborder status updated successfully",
                    Data = updatedSubOrder
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for suborder {SubOrderId}", id);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while updating the suborder status",
                    ErrorMessages = new[] { ex.Message }
                });
            }
        }
        // GET: api/orders/{id}
        [HttpGet("{id}")]
        [EnableRateLimiting("standard")]
        [ResponseCache(Duration = 60)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                // Create cache key
                //var cacheKey = $"order_{id}";

                //// Try to get from cache first
                //if (_cache.TryGetValue(cacheKey, out ApiResponse<OrderDto> cachedResult))
                //{
                //    return Ok(cachedResult);
                //}

                var order = await _orderRepository.GetOrderByIdAsync(id);

                if (order == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Order not found",
                        Success = false,
                        Data = null
                    });
                }

                var response = new ApiResponse<OrderDto>
                {
                    Message = "Order retrieved successfully",
                    Data = order,
                    Success = true
                };

                // Cache the result
                //var cacheEntryOptions = new MemoryCacheEntryOptions()
                //    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                //    .SetSlidingExpiration(TimeSpan.FromMinutes(1));

                //_cache.Set(cacheKey, response, cacheEntryOptions);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving order {OrderId}", id);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = $"An error occurred while retrieving order with id = {id}",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // POST: api/orders
        [HttpPost]
        [EnableRateLimiting("strict")]
        public async Task<IActionResult> Create([FromBody] CreateOrderInputDto orderDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Invalid order data",
                    ErrorMessages = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToArray()
                });
            }

            try
            {
                var createdOrder = await _orderRepository.CreateOrderAsync(orderDto);

                // Invalidate cache for customer's orders
                // InvalidateCustomerOrdersCache(orderDto.CustomerId);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = createdOrder.OrderId },
                    new ApiResponse<OrderDto>
                    {
                        Message = "Order created successfully",
                        Data = createdOrder,
                        Success = true
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating order for customer {CustomerId}", orderDto.CustomerId);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while creating the order",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // PUT: api/orders/{id}
        [HttpPut("{id}")]
        [EnableRateLimiting("strict")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateOrderInputDto orderDto)
        {
            if (id != orderDto.OrderId)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Invalid order ID",
                    ErrorMessages = new string[] { "The order ID in the URL does not match the ID in the body" }
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Invalid order data",
                    ErrorMessages = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToArray()
                });
            }

            try
            {
                if (!await _orderRepository.OrderExistsAsync(id))
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Order not found",
                        Success = false,
                        Data = null
                    });
                }

                var updatedOrder = await _orderRepository.UpdateOrderAsync(id, orderDto);

                // Invalidate caches
                //InvalidateOrderCache(id);
                //// Since we don't know the customer ID here, we'll invalidate all customer-related caches
                //InvalidateAllOrderCaches();

                return Ok(new ApiResponse<OrderDto>
                {
                    Message = "Order updated successfully",
                    Data = updatedOrder,
                    Success = true
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ApiResponse<object>
                {
                    Message = "Order not found",
                    Success = false,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating order {OrderId}", id);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while updating the order",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // PATCH: api/orders/{id}/payment-status
        [HttpPatch("{id}/payment-status")]
        [EnableRateLimiting("strict")]
        public async Task<IActionResult> UpdatePaymentStatus(int id, [FromBody] string paymentStatus)
        {
            if (string.IsNullOrEmpty(paymentStatus))
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Payment status cannot be empty",
                    ErrorMessages = new string[] { "A valid payment status must be provided" }
                });
            }

            try
            {
                if (!await _orderRepository.OrderExistsAsync(id))
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Order not found",
                        Success = false,
                        Data = null
                    });
                }

                var updatedOrder = await _orderRepository.UpdateOrderPaymentStatusAsync(id, paymentStatus);

                // Invalidate caches
                //InvalidateOrderCache(id);
                //InvalidateAllOrderCaches();

                return Ok(new ApiResponse<OrderDto>
                {
                    Message = "Order payment status updated successfully",
                    Data = updatedOrder,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating payment status for order {OrderId}", id);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while updating the order payment status",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // DELETE: api/orders/{id}
        [HttpDelete("{id}")]
        [EnableRateLimiting("strict")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (!await _orderRepository.OrderExistsAsync(id))
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Order not found",
                        Success = false,
                        Data = null
                    });
                }

                var result = await _orderRepository.DeleteOrderAsync(id);

                // Invalidate caches
                //InvalidateOrderCache(id);
                //InvalidateAllOrderCaches();

                return Ok(new ApiResponse<object>
                {
                    Message = "Order deleted successfully",
                    Data = null,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting order {OrderId}", id);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while deleting the order",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // GET: api/orders/sub-orders/{id}
        [HttpGet("sub-orders/{id}")]
        [EnableRateLimiting("standard")]
        [ResponseCache(Duration = 60)]
        public async Task<IActionResult> GetSubOrderById(int id)
        {
            try
            {
                // Create cache key
                //var cacheKey = $"suborder_{id}";

                //// Try to get from cache first
                //if (_cache.TryGetValue(cacheKey, out ApiResponse<SubOrderDto> cachedResult))
                //{
                //    return Ok(cachedResult);
                //}

                var subOrder = await _orderRepository.GetSubOrderByIdAsync(id);

                if (subOrder == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "SubOrder not found",
                        Success = false,
                        Data = null
                    });
                }

                var response = new ApiResponse<SubOrderDto>
                {
                    Message = "SubOrder retrieved successfully",
                    Data = subOrder,
                    Success = true
                };

                // Cache the result
                //var cacheEntryOptions = new MemoryCacheEntryOptions()
                //    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                //    .SetSlidingExpiration(TimeSpan.FromMinutes(1));

                //_cache.Set(cacheKey, response, cacheEntryOptions);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving sub-order {SubOrderId}", id);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = $"An error occurred while retrieving sub-order with id = {id}",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // GET: api/orders/seller/{sellerId}
        [HttpGet("seller/{sellerId}")]
        [EnableRateLimiting("standard")]
        [ResponseCache(Duration = 60)]
        public async Task<IActionResult> GetSubOrdersBySellerId(int sellerId, [FromQuery] PaginationDto pagination)
        {
            try
            {
                // Create a cache key based on parameters
                var cacheKey = $"suborders_seller_{sellerId}_{pagination.PageNumber}_{pagination.PageSize}";

                // Try to get from cache first
                //if (_cache.TryGetValue(cacheKey, out ApiResponse<object> cachedResult))
                //{
                //    return Ok(cachedResult);
                //}

                // Make sure pagination is valid
                if (pagination.PageNumber < 1)
                    pagination.PageNumber = 1;

                if (pagination.PageSize < 1)
                    pagination.PageSize = 10;

                var subOrders = await _orderRepository.GetSubOrdersBySellerIdAsync(sellerId, pagination);
                var totalCount = await _orderRepository.GetSubOrdersCountBySellerIdAsync(sellerId);

                var response = new ApiResponse<object>
                {
                    Message = "Successfully retrieved sub-orders for seller",
                    Data = new
                    {
                        Items = subOrders,
                        TotalCount = totalCount,
                        PageNumber = pagination.PageNumber,
                        PageSize = pagination.PageSize,
                        TotalPages = (int)Math.Ceiling(totalCount / (double)pagination.PageSize),
                        HasPreviousPage = pagination.PageNumber > 1,
                        HasNextPage = pagination.PageNumber < (int)Math.Ceiling(totalCount / (double)pagination.PageSize)
                    },
                    Success = true
                };

                // Cache the result
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(1));

                //_cache.Set(cacheKey, response, cacheEntryOptions);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving sub-orders for seller {SellerId}", sellerId);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving seller sub-orders",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // PUT: api/orders/sub-orders/{id}
        [HttpPut("sub-orders/{id}")]
        [EnableRateLimiting("strict")]
        public async Task<IActionResult> UpdateSubOrderStatus(int id, [FromBody] UpdateSubOrderInputDto subOrderDto)
        {
            if (id != subOrderDto.SuborderId)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Invalid sub-order ID",
                    ErrorMessages = new string[] { "The sub-order ID in the URL does not match the ID in the body" }
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Invalid sub-order data",
                    ErrorMessages = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToArray()
                });
            }

            try
            {
                var subOrder = await _orderRepository.GetSubOrderByIdAsync(id);
                if (subOrder == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Sub-order not found",
                        Success = false,
                        Data = null
                    });
                }

                var updatedSubOrder = await _orderRepository.UpdateSubOrderStatusAsync(id, subOrderDto);

                // Invalidate caches
                //InvalidateSubOrderCache(id);
                //InvalidateSellerSubOrdersCache(subOrder.SellerId);
                //InvalidateOrderCache(subOrder.OrderId);
                //InvalidateAllOrderCaches();

                return Ok(new ApiResponse<SubOrderDto>
                {
                    Message = "Sub-order updated successfully",
                    Data = updatedSubOrder,
                    Success = true
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ApiResponse<object>
                {
                    Message = "Sub-order not found",
                    Success = false,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating sub-order {SubOrderId}", id);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while updating the sub-order",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // GET: api/orders/status/{status}
        [HttpGet("status/{status}")]
        [EnableRateLimiting("standard")]
        [ResponseCache(Duration = 60)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOrdersByPaymentStatus(string status, [FromQuery] PaginationDto pagination)
        {
            try
            {
                // Create a cache key based on parameters
                var cacheKey = $"orders_payment_status_{status}_{pagination.PageNumber}_{pagination.PageSize}";

                // Try to get from cache first
                //if (_cache.TryGetValue(cacheKey, out ApiResponse<object> cachedResult))
                //{
                //    return Ok(cachedResult);
                //}

                // Make sure pagination is valid
                if (pagination.PageNumber < 1)
                    pagination.PageNumber = 1;

                if (pagination.PageSize < 1)
                    pagination.PageSize = 10;

                var orders = await _orderRepository.GetOrdersByPaymentStatusAsync(status, pagination);
                var totalCount = await _orderRepository.GetOrdersCountByPaymentStatusAsync(status);

                var response = new ApiResponse<object>
                {
                    Message = $"Successfully retrieved orders with payment status: {status}",
                    Data = new
                    {
                        Items = orders,
                        TotalCount = totalCount,
                        PageNumber = pagination.PageNumber,
                        PageSize = pagination.PageSize,
                        TotalPages = (int)Math.Ceiling(totalCount / (double)pagination.PageSize),
                        HasPreviousPage = pagination.PageNumber > 1,
                        HasNextPage = pagination.PageNumber < (int)Math.Ceiling(totalCount / (double)pagination.PageSize)
                    },
                    Success = true
                };

                // Cache the result
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(1));

                //_cache.Set(cacheKey, response, cacheEntryOptions);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving orders with payment status {Status}", status);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving orders by payment status",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // GET: api/orders/sub-orders/status/{status}
        [HttpGet("sub-orders/status/{status}")]
        [EnableRateLimiting("standard")]
        [ResponseCache(Duration = 60)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSubOrdersByStatus(string status, [FromQuery] PaginationDto pagination)
        {
            try
            {
                // Create a cache key based on parameters
                var cacheKey = $"suborders_status_{status}_{pagination.PageNumber}_{pagination.PageSize}";

                // Try to get from cache first
                //if (_cache.TryGetValue(cacheKey, out ApiResponse<object> cachedResult))
                //{
                //    return Ok(cachedResult);
                //}

                // Make sure pagination is valid
                if (pagination.PageNumber < 1)
                    pagination.PageNumber = 1;

                if (pagination.PageSize < 1)
                    pagination.PageSize = 10;

                var subOrders = await _orderRepository.GetSubOrdersByStatusAsync(status, pagination);
                var totalCount = await _orderRepository.GetSubOrdersCountByStatusAsync(status);

                var response = new ApiResponse<object>
                {
                    Message = $"Successfully retrieved sub-orders with status: {status}",
                    Data = new
                    {
                        Items = subOrders,
                        TotalCount = totalCount,
                        PageNumber = pagination.PageNumber,
                        PageSize = pagination.PageSize,
                        TotalPages = (int)Math.Ceiling(totalCount / (double)pagination.PageSize),
                        HasPreviousPage = pagination.PageNumber > 1,
                        HasNextPage = pagination.PageNumber < (int)Math.Ceiling(totalCount / (double)pagination.PageSize)
                    },
                    Success = true
                };

                // Cache the result
                //var cacheEntryOptions = new MemoryCacheEntryOptions()
                //    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                //    .SetSlidingExpiration(TimeSpan.FromMinutes(1));

                //_cache.Set(cacheKey, response, cacheEntryOptions);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving sub-orders with status {Status}", status);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving sub-orders by status",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // Cache invalidation helpers
        //private void InvalidateOrderCache(int orderId)
        //{
        //    var cacheKey = $"order_{orderId}";
        //    _cache.Remove(cacheKey);
        //}

        //private void InvalidateSubOrderCache(int subOrderId)
        //{
        //    var cacheKey = $"suborder_{subOrderId}";
        //    _cache.Remove(cacheKey);
        //}

        //private void InvalidateCustomerOrdersCache(int customerId)
        //{
        //    var listCacheKeyPattern = $"orders_customer_{customerId}_";
        //    _cache.Remove(listCacheKeyPattern);
        //}

        //private void InvalidateSellerSubOrdersCache(int sellerId)
        //{
        //    var listCacheKeyPattern = $"suborders_seller_{sellerId}_";
        //    _cache.Remove(listCacheKeyPattern);
        //}

        //private void InvalidateAllOrderCaches()
        //{
        //    // This is a simplified approach - in production you might want to be more targeted
        //    _cache.Remove("orders_all_");
        //    _cache.Remove("orders_payment_status_");
        //    _cache.Remove("suborders_status_");
        //}
    }
}