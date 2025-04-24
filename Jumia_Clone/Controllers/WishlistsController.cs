using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Repositories.Interfaces;
using System.Security.Claims;
using Jumia_Clone.Models.DTOs.WishlistItemDTOs;

namespace Jumia_Clone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WishlistsController : ControllerBase
    {
        private readonly IWishlistRepository _wishlistRepository;
        ////private readonly IMemoryCache _cache;
        private readonly ILogger<WishlistsController> _logger;

        public WishlistsController(
            IWishlistRepository wishlistRepository,
            IMemoryCache cache,
            ILogger<WishlistsController> logger)
        {
            _wishlistRepository = wishlistRepository;
            //_cache = cache;
            _logger = logger;
        }

        #region Helper Methods

        private int GetCurrentCustomerId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("Unable to determine current user");
            }

            return userId;
        }

        //private void InvalidateWishlistCache(int customerId)
        //{
        //    var wishlistCacheKey = $"wishlist_{customerId}";
        //    var wishlistSummaryCacheKey = $"wishlist_summary_{customerId}";
        //    var recentlyAddedCacheKey = $"wishlist_recent_{customerId}";
        //    var outOfStockCacheKey = $"wishlist_outofstock_{customerId}";

        //    //_cache.Remove(wishlistCacheKey);
        //    //_cache.Remove(wishlistSummaryCacheKey);
        //    //_cache.Remove(recentlyAddedCacheKey);
        //    //_cache.Remove(outOfStockCacheKey);
        //}

        #endregion

        // GET: api/wishlists
        [HttpGet]
        [EnableRateLimiting("standard")]
        [ResponseCache(Duration = 30)]
        public async Task<IActionResult> GetMyWishlist()
        {
            try
            {
                int customerId = GetCurrentCustomerId();
                var cacheKey = $"wishlist_{customerId}";

                // Try to get from cache first
                //if (_cache.TryGetValue(cacheKey, out ApiResponse<WishlistDto> cachedResult))
                //{
                //    return Ok(cachedResult);
                //}

                var wishlist = await _wishlistRepository.GetWishlistByCustomerIdAsync(customerId);

                var response = new ApiResponse<WishlistDto>
                {
                    Message = "Wishlist retrieved successfully",
                    Data = wishlist,
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
                _logger.LogError(ex, "An error occurred while retrieving wishlist");
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving wishlist",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // GET: api/wishlists/summary
        [HttpGet("summary")]
        [EnableRateLimiting("standard")]
        [ResponseCache(Duration = 30)]
        public async Task<IActionResult> GetWishlistSummary()
        {
            try
            {
                int customerId = GetCurrentCustomerId();
                var cacheKey = $"wishlist_summary_{customerId}";

                // Try to get from cache first
                //if (_cache.TryGetValue(cacheKey, out ApiResponse<WishlistSummaryDto> cachedResult))
                //{
                //    return Ok(cachedResult);
                //}

                var summary = await _wishlistRepository.GetWishlistSummaryByCustomerIdAsync(customerId);

                var response = new ApiResponse<WishlistSummaryDto>
                {
                    Message = "Wishlist summary retrieved successfully",
                    Data = summary,
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
                _logger.LogError(ex, "An error occurred while retrieving wishlist summary");
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving wishlist summary",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // GET: api/wishlists/items/{id}
        [HttpGet("items/{id}")]
        [EnableRateLimiting("standard")]
        public async Task<IActionResult> GetWishlistItem(int id)
        {
            try
            {
                int customerId = GetCurrentCustomerId();

                // Check if the item belongs to the customer
                if (!await _wishlistRepository.WishlistItemBelongsToCustomerAsync(customerId, id))
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Wishlist item not found",
                        Success = false,
                        Data = null
                    });
                }

                var wishlistItem = await _wishlistRepository.GetWishlistItemByIdAsync(id);

                return Ok(new ApiResponse<WishlistItemDto>
                {
                    Message = "Wishlist item retrieved successfully",
                    Data = wishlistItem,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving wishlist item");
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving wishlist item",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // GET: api/wishlists/recent
        [HttpGet("recent")]
        [EnableRateLimiting("standard")]
        [ResponseCache(Duration = 30)]
        public async Task<IActionResult> GetRecentlyAddedItems([FromQuery] int count = 5)
        {
            try
            {
                int customerId = GetCurrentCustomerId();
                var cacheKey = $"wishlist_recent_{customerId}_{count}";

                // Try to get from cache first
                //if (_cache.TryGetValue(cacheKey, out ApiResponse<List<WishlistItemDto>> cachedResult))
                //{
                //    return Ok(cachedResult);
                //}

                var recentItems = await _wishlistRepository.GetRecentlyAddedWishlistItemsAsync(customerId, count);

                var response = new ApiResponse<List<WishlistItemDto>>
                {
                    Message = "Recently added wishlist items retrieved successfully",
                    Data = recentItems,
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
                _logger.LogError(ex, "An error occurred while retrieving recently added wishlist items");
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving recently added wishlist items",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // GET: api/wishlists/out-of-stock
        [HttpGet("out-of-stock")]
        [EnableRateLimiting("standard")]
        [ResponseCache(Duration = 30)]
        public async Task<IActionResult> GetOutOfStockItems()
        {
            try
            {
                int customerId = GetCurrentCustomerId();
                var cacheKey = $"wishlist_outofstock_{customerId}";

                // Try to get from cache first
                //if (_cache.TryGetValue(cacheKey, out ApiResponse<List<WishlistItemDto>> cachedResult))
                //{
                //    return Ok(cachedResult);
                //}

                var outOfStockItems = await _wishlistRepository.GetOutOfStockWishlistItemsAsync(customerId);

                var response = new ApiResponse<List<WishlistItemDto>>
                {
                    Message = "Out of stock wishlist items retrieved successfully",
                    Data = outOfStockItems,
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
                _logger.LogError(ex, "An error occurred while retrieving out of stock wishlist items");
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving out of stock wishlist items",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // POST: api/wishlists/items
        [HttpPost("items")]
        [EnableRateLimiting("strict")]
        public async Task<IActionResult> AddToWishlist([FromBody] AddWishlistItemDto itemDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Invalid wishlist item data",
                    ErrorMessages = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToArray()
                });
            }

            try
            {
                int customerId = GetCurrentCustomerId();

                // Check if item already exists in wishlist
                if (await _wishlistRepository.WishlistItemExistsAsync(customerId, itemDto.ProductId))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Message = "Product already exists in wishlist",
                        Success = false,
                        Data = null
                    });
                }

                var wishlistItem = await _wishlistRepository.AddItemToWishlistAsync(customerId, itemDto);

                // Invalidate cache
                //InvalidateWishlistCache(customerId);

                return CreatedAtAction(
                    nameof(GetWishlistItem),
                    new { id = wishlistItem.WishlistItemId },
                    new ApiResponse<WishlistItemDto>
                    {
                        Message = "Item added to wishlist successfully",
                        Data = wishlistItem,
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding item to wishlist");
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while adding item to wishlist",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // DELETE: api/wishlists/items/{id}
        [HttpDelete("items/{id}")]
        [EnableRateLimiting("strict")]
        public async Task<IActionResult> RemoveFromWishlist(int id)
        {
            try
            {
                int customerId = GetCurrentCustomerId();

                // Check if the item belongs to the customer
                if (!await _wishlistRepository.WishlistItemBelongsToCustomerAsync(customerId, id))
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Wishlist item not found",
                        Success = false,
                        Data = null
                    });
                }

                var result = await _wishlistRepository.RemoveItemFromWishlistAsync(id);

                // Invalidate cache
                //InvalidateWishlistCache(customerId);

                return Ok(new ApiResponse<object>
                {
                    Message = "Item removed from wishlist successfully",
                    Success = true,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while removing item from wishlist");
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while removing item from wishlist",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // DELETE: api/wishlists/clear
        [HttpDelete("clear")]
        [EnableRateLimiting("strict")]
        public async Task<IActionResult> ClearWishlist()
        {
            try
            {
                int customerId = GetCurrentCustomerId();

                var result = await _wishlistRepository.ClearWishlistAsync(customerId);

                // Invalidate cache
                //InvalidateWishlistCache(customerId);

                return Ok(new ApiResponse<object>
                {
                    Message = "Wishlist cleared successfully",
                    Success = true,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while clearing wishlist");
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while clearing wishlist",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // POST: api/wishlists/move-to-cart/{id}
        [HttpPost("move-to-cart/{id}")]
        [EnableRateLimiting("strict")]
        public async Task<IActionResult> MoveToCart(int id)
        {
            try
            {
                int customerId = GetCurrentCustomerId();

                // Check if the item belongs to the customer
                if (!await _wishlistRepository.WishlistItemBelongsToCustomerAsync(customerId, id))
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Wishlist item not found",
                        Success = false,
                        Data = null
                    });
                }

                var cartItem = await _wishlistRepository.MoveItemToCartAsync(customerId, id);

                // Invalidate cache
                //InvalidateWishlistCache(customerId);

                return Ok(new ApiResponse<object>
                {
                    Message = "Item moved to cart successfully",
                    Success = true,
                    Data = cartItem
                });
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
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Message = ex.Message,
                    Success = false,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while moving item to cart");
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while moving item to cart",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // POST: api/wishlists/move-multiple-to-cart
        [HttpPost("move-multiple-to-cart")]
        [EnableRateLimiting("strict")]
        public async Task<IActionResult> MoveMultipleToCart([FromBody] MoveMultipleToCartDto moveDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Invalid data",
                    ErrorMessages = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToArray()
                });
            }

            try
            {
                int customerId = GetCurrentCustomerId();

                // Filter out items that don't belong to this customer
                var validItemIds = new List<int>();
                foreach (var itemId in moveDto.WishlistItemIds)
                {
                    if (await _wishlistRepository.WishlistItemBelongsToCustomerAsync(customerId, itemId))
                    {
                        validItemIds.Add(itemId);
                    }
                }

                if (validItemIds.Count == 0)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "No valid wishlist items found",
                        Success = false,
                        Data = null
                    });
                }

                var cartItems = await _wishlistRepository.MoveMultipleItemsToCartAsync(customerId, validItemIds);

                // Invalidate cache
                ////InvalidateWishlistCache(customerId);

                return Ok(new ApiResponse<object>
                {
                    Message = $"{cartItems.Count} item(s) moved to cart successfully",
                    Success = true,
                    Data = new
                    {
                        MovedItems = cartItems,
                        TotalMoved = cartItems.Count,
                        RequestedCount = moveDto.WishlistItemIds.Count
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while moving multiple items to cart");
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while moving items to cart",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }
    }
}