using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Models.DTOs.CartDTOs;
using Jumia_Clone.Repositories.Interfaces;
using System.Security.Claims;
using Jumia_Clone.CustomException;

namespace Jumia_Clone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartsController : ControllerBase
    {
        private readonly ICartRepository _cartRepository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CartsController> _logger;

        public CartsController(
            ICartRepository cartRepository,
            IMemoryCache cache,
            ILogger<CartsController> logger)
        {
            _cartRepository = cartRepository;
            _cache = cache;
            _logger = logger;
        }

        // Helper method to get current customer ID
        private int GetCurrentCustomerId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("Unable to determine current user");
            }

            return userId;
        }

        // GET: api/carts
        [HttpGet]
        [EnableRateLimiting("standard")]
        [ResponseCache(Duration = 30)]
        public async Task<IActionResult> GetCart()
        {
            try
            {
                int customerId = GetCurrentCustomerId();
                var cacheKey = $"cart_{customerId}";

                // Try to get from cache first
                if (_cache.TryGetValue(cacheKey, out ApiResponse<CartDto> cachedResult))
                {
                    return Ok(cachedResult);
                }

                var cart = await _cartRepository.GetCartByCustomerIdAsync(customerId);

                var response = new ApiResponse<CartDto>
                {
                    Message = "Cart retrieved successfully",
                    Data = cart,
                    Success = true
                };

                // Cache the result
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(1));

                _cache.Set(cacheKey, response, cacheEntryOptions);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving cart");
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving cart",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // GET: api/carts/summary
        [HttpGet("summary")]
        [EnableRateLimiting("standard")]
        [ResponseCache(Duration = 30)]
        public async Task<IActionResult> GetCartSummary()
        {
            try
            {
                int customerId = GetCurrentCustomerId();
                var cacheKey = $"cart_summary_{customerId}";

                // Try to get from cache first
                if (_cache.TryGetValue(cacheKey, out ApiResponse<CartSummaryDto> cachedResult))
                {
                    return Ok(cachedResult);
                }

                var cartSummary = await _cartRepository.GetCartSummaryByCustomerIdAsync(customerId);

                var response = new ApiResponse<CartSummaryDto>
                {
                    Message = "Cart summary retrieved successfully",
                    Data = cartSummary,
                    Success = true
                };

                // Cache the result
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(1));

                _cache.Set(cacheKey, response, cacheEntryOptions);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving cart summary");
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving cart summary",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // GET: api/carts/items/{id}
        [HttpGet("items/{id}")]
        [EnableRateLimiting("standard")]
        public async Task<IActionResult> GetCartItem(int id)
        {
            try
            {
                int customerId = GetCurrentCustomerId();

                // Check if the item belongs to the customer
                if (!await _cartRepository.CartItemExistsAndBelongsToCustomerAsync(customerId, id))
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Cart item not found",
                        Success = false,
                        Data = null
                    });
                }

                var cartItem = await _cartRepository.GetCartItemByIdAsync(id);

                return Ok(new ApiResponse<CartItemDto>
                {
                    Message = "Cart item retrieved successfully",
                    Data = cartItem,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving cart item");
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving cart item",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // POST: api/carts/items
        [HttpPost("items")]
        [EnableRateLimiting("strict")]
        public async Task<IActionResult> AddCartItem([FromBody] AddCartItemDto cartItemDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Invalid cart item data",
                    ErrorMessages = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToArray()
                });
            }

            try
            {
                int customerId = GetCurrentCustomerId();

                var cartItem = await _cartRepository.AddCartItemAsync(customerId, cartItemDto);

                // Invalidate cache
                InvalidateCartCache(customerId);

                return CreatedAtAction(
                    nameof(GetCartItem),
                    new { id = cartItem.CartItemId },
                    new ApiResponse<CartItemDto>
                    {
                        Message = "Item added to cart successfully",
                        Data = cartItem,
                        Success = true
                    }
                );
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>
                {
                    Message = ex.Message,
                    Success = false,
                    Data = null
                });
            }
            catch (InsufficientStockException ex)
            {
                _logger.LogError(ex, "An error occurred while updating cart item");
                return BadRequest(new ApiErrorResponse
                {
                    Message = "An error occurred while updating cart item",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding item to cart");
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while adding item to cart",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // PUT: api/carts/items/{id}
        [HttpPut("items/{id}")]
        [EnableRateLimiting("strict")]
        public async Task<IActionResult> UpdateCartItem(int id, [FromBody] UpdateCartItemDto updateCartItemDto)
        {
            if (id != updateCartItemDto.CartItemId)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "ID mismatch",
                    ErrorMessages = new string[] { "The cart item ID in the URL does not match the ID in the body" }
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Invalid cart item data",
                    ErrorMessages = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToArray()
                });
            }

            try
            {
                int customerId = GetCurrentCustomerId();

                // Check if the item belongs to the customer
                if (!await _cartRepository.CartItemExistsAndBelongsToCustomerAsync(customerId, id))
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Cart item not found",
                        Success = false,
                        Data = null
                    });
                }

                var cartItem = await _cartRepository.UpdateCartItemQuantityAsync(customerId, updateCartItemDto);

                // Invalidate cache
                InvalidateCartCache(customerId);

                return Ok(new ApiResponse<CartItemDto>
                {
                    Message = "Cart item updated successfully",
                    Data = cartItem,
                    Success = true
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ApiResponse<object>
                {
                    Message = "Cart item not found",
                    Success = false,
                    Data = null
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InsufficientStockException ex)
            {
                _logger.LogError(ex, "An error occurred while updating cart item");
                return BadRequest(new ApiErrorResponse
                {
                    Message = "An error occurred while updating cart item",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating cart item");
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while updating cart item",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // DELETE: api/carts/items/{id}
        [HttpDelete("items/{id}")]
        [EnableRateLimiting("strict")]
        public async Task<IActionResult> RemoveCartItem(int id)
        {
            try
            {
                int customerId = GetCurrentCustomerId();

                // Check if the item belongs to the customer
                if (!await _cartRepository.CartItemExistsAndBelongsToCustomerAsync(customerId, id))
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Cart item not found",
                        Success = false,
                        Data = null
                    });
                }

                var result = await _cartRepository.RemoveCartItemAsync(customerId, id);

                // Invalidate cache
                InvalidateCartCache(customerId);

                return Ok(new ApiResponse<object>
                {
                    Message = "Cart item removed successfully",
                    Success = true,
                    Data = null
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while removing cart item");
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while removing cart item",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // DELETE: api/carts/clear
        [HttpDelete("clear")]
        [EnableRateLimiting("strict")]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                int customerId = GetCurrentCustomerId();

                var result = await _cartRepository.ClearCartAsync(customerId);

                // Invalidate cache
                InvalidateCartCache(customerId);

                return Ok(new ApiResponse<object>
                {
                    Message = "Cart cleared successfully",
                    Success = true,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while clearing cart");
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while clearing cart",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // Helper method to invalidate cart cache
        private void InvalidateCartCache(int customerId)
        {
            var cartCacheKey = $"cart_{customerId}";
            var summaryCartCacheKey = $"cart_summary_{customerId}";

            _cache.Remove(cartCacheKey);
            _cache.Remove(summaryCartCacheKey);
        }
    }
}