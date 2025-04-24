using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Models.DTOs.CouponDTOs;
using Jumia_Clone.Repositories.Interfaces;
using System.Security.Claims;

namespace Jumia_Clone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CouponsController : ControllerBase
    {
        private readonly ICouponRepository _couponRepository;
        ////private readonly IMemoryCache _cache;
        private readonly ILogger<CouponsController> _logger;

        public CouponsController(
            ICouponRepository couponRepository,
            IMemoryCache cache,
            ILogger<CouponsController> logger)
        {
            _couponRepository = couponRepository;
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

        private bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }

        private void InvalidateCouponCache(int couponId = 0)
        {
            if (couponId > 0)
            {
                //_cache.Remove($"coupon_{couponId}");
            }
            //_cache.Remove("coupons_list");
        }

        #endregion

        #region Coupon Management (Admin)

        // GET: api/coupons
        [HttpGet]
        [EnableRateLimiting("standard")]
        [ResponseCache(Duration = 60)]
        public async Task<IActionResult> GetAllCoupons(
            [FromQuery] PaginationDto pagination,
            [FromQuery] bool? isActive = null,
            [FromQuery] string searchTerm = null)
        {
            try
            {
                // Create a cache key based on parameters
                var cacheKey = $"coupons_list_{pagination.PageNumber}_{pagination.PageSize}_{isActive}_{searchTerm}";

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

                var coupons = await _couponRepository.GetAllCouponsAsync(pagination, isActive, searchTerm);
                var totalCount = await _couponRepository.GetCouponsCountAsync(isActive, searchTerm);

                var response = new ApiResponse<object>
                {
                    Message = "Successfully retrieved coupons",
                    Data = new
                    {
                        Items = coupons,
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
                _logger.LogError(ex, "An error occurred while retrieving coupons");
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving coupons",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // GET: api/coupons/{id}
        [HttpGet("{id}")]
        [EnableRateLimiting("standard")]
        [ResponseCache(Duration = 60)]
        public async Task<IActionResult> GetCouponById(int id)
        {
            try
            {
                var cacheKey = $"coupon_{id}";

                // Try to get from cache first
                //if (_cache.TryGetValue(cacheKey, out ApiResponse<CouponDto> cachedResult))
                //{
                //    return Ok(cachedResult);
                //}

                var coupon = await _couponRepository.GetCouponByIdAsync(id);

                if (coupon == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Coupon not found",
                        Success = false,
                        Data = null
                    });
                }

                var response = new ApiResponse<CouponDto>
                {
                    Message = "Coupon retrieved successfully",
                    Data = coupon,
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
                _logger.LogError(ex, "An error occurred while retrieving coupon {CouponId}", id);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = $"An error occurred while retrieving coupon with id = {id}",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // POST: api/coupons
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("strict")]
        public async Task<IActionResult> CreateCoupon([FromBody] CreateCouponDto createCouponDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Invalid coupon data",
                    ErrorMessages = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToArray()
                });
            }

            try
            {
                // Check if coupon code already exists
                if (await _couponRepository.CouponCodeExistsAsync(createCouponDto.Code))
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        Message = "Coupon creation failed",
                        ErrorMessages = new string[] { $"Coupon code '{createCouponDto.Code}' already exists" }
                    });
                }

                var coupon = await _couponRepository.CreateCouponAsync(createCouponDto);

                // Invalidate cache
                InvalidateCouponCache();

                return CreatedAtAction(
                    nameof(GetCouponById),
                    new { id = coupon.CouponId },
                    new ApiResponse<CouponDto>
                    {
                        Message = "Coupon created successfully",
                        Data = coupon,
                        Success = true
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating coupon");
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while creating coupon",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // PUT: api/coupons/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("strict")]
        public async Task<IActionResult> UpdateCoupon(int id, [FromBody] UpdateCouponDto updateCouponDto)
        {
            if (id != updateCouponDto.CouponId)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Invalid coupon ID",
                    ErrorMessages = new string[] { "The coupon ID in the URL does not match the ID in the body" }
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Invalid coupon data",
                    ErrorMessages = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToArray()
                });
            }

            try
            {
                if (!await _couponRepository.CouponExistsAsync(id))
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Coupon not found",
                        Success = false,
                        Data = null
                    });
                }

                var coupon = await _couponRepository.UpdateCouponAsync(updateCouponDto);

                // Invalidate cache
                InvalidateCouponCache(id);

                return Ok(new ApiResponse<CouponDto>
                {
                    Message = "Coupon updated successfully",
                    Data = coupon,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating coupon {CouponId}", id);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while updating the coupon",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // DELETE: api/coupons/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("strict")]
        public async Task<IActionResult> DeleteCoupon(int id)
        {
            try
            {
                if (!await _couponRepository.CouponExistsAsync(id))
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Coupon not found",
                        Success = false,
                        Data = null
                    });
                }

                var result = await _couponRepository.DeleteCouponAsync(id);

                // Invalidate cache
                InvalidateCouponCache(id);

                return Ok(new ApiResponse<object>
                {
                    Message = "Coupon deleted successfully",
                    Success = true,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting coupon {CouponId}", id);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while deleting the coupon",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        #endregion

        #region Coupon Validation and Usage (Customer)

        // POST: api/coupons/validate
        [HttpPost("validate")]
        [Authorize]
        [EnableRateLimiting("standard")]
        public async Task<IActionResult> ValidateCoupon([FromBody] ValidateCouponDto validateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Invalid validation data",
                    ErrorMessages = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToArray()
                });
            }

            try
            {
                // Ensure customer ID matches current user if not admin
                int currentCustomerId = GetCurrentCustomerId();
                if (!IsAdmin() && validateDto.CustomerId != currentCustomerId)
                {
                    return Forbid();
                }

                var validationResult = await _couponRepository.ValidateCouponAsync(validateDto);

                return Ok(new ApiResponse<CouponValidationResultDto>
                {
                    Message = validationResult.IsValid ?
                        "Coupon is valid" :
                        "Coupon validation failed",
                    Data = validationResult,
                    Success = validationResult.IsValid
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while validating coupon");
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while validating coupon",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        #endregion

        #region User Coupon Management

        // GET: api/coupons/user
        [HttpGet("user")]
        [Authorize]
        [EnableRateLimiting("standard")]
        public async Task<IActionResult> GetUserCoupons([FromQuery] bool includeUsed = false)
        {
            try
            {
                int customerId = GetCurrentCustomerId();
                var cacheKey = $"user_coupons_{customerId}_{includeUsed}";

                // Try to get from cache first
                //if (_cache.TryGetValue(cacheKey, out ApiResponse<IEnumerable<UserCouponDto>> cachedResult))
                //{
                //    return Ok(cachedResult);
                //}

                var userCoupons = await _couponRepository.GetUserCouponsAsync(customerId, includeUsed);

                var response = new ApiResponse<IEnumerable<UserCouponDto>>
                {
                    Message = "User coupons retrieved successfully",
                    Data = userCoupons,
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
                _logger.LogError(ex, "An error occurred while retrieving user coupons");
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving user coupons",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // POST: api/coupons/assign
        [HttpPost("assign")]
        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("strict")]
        public async Task<IActionResult> AssignCouponToUser([FromBody] AssignCouponDto assignDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Invalid assignment data",
                    ErrorMessages = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToArray()
                });
            }

            try
            {
                // Check if coupon exists
                if (!await _couponRepository.CouponExistsAsync(assignDto.CouponId))
                {
                    return NotFound(new ApiErrorResponse
                    {
                        Message = "Coupon not found",
                        ErrorMessages = new string[] { $"Coupon with ID {assignDto.CouponId} not found" }
                    });
                }

                // Check if user already has this coupon
                if (await _couponRepository.UserHasCouponAsync(assignDto.CustomerId, assignDto.CouponId))
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        Message = "Assignment failed",
                        ErrorMessages = new string[] { "Customer already has this coupon" }
                    });
                }

                var userCoupon = await _couponRepository.AssignCouponToUserAsync(assignDto);

                // Invalidate user coupons cache
                //_cache.Remove($"user_coupons_{assignDto.CustomerId}_false");
                //_cache.Remove($"user_coupons_{assignDto.CustomerId}_true");

                return Ok(new ApiResponse<UserCouponDto>
                {
                    Message = "Coupon assigned to user successfully",
                    Data = userCoupon,
                    Success = true
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse
                {
                    Message = "Assignment failed",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Assignment failed",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while assigning coupon to user");
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while assigning coupon to user",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // POST: api/coupons/mark-used/{id}
        [HttpPost("mark-used/{id}")]
        [Authorize]
        [EnableRateLimiting("strict")]
        public async Task<IActionResult> MarkCouponAsUsed(int id)
        {
            try
            {
                var userCoupon = await _couponRepository.GetUserCouponByIdAsync(id);

                if (userCoupon == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "User coupon not found",
                        Success = false,
                        Data = null
                    });
                }

                // Make sure the current user owns this coupon or is an admin
                int currentCustomerId = GetCurrentCustomerId();
                if (!IsAdmin() && userCoupon.CustomerId != currentCustomerId)
                {
                    return Forbid();
                }

                // Check if already used
                if (userCoupon.IsUsed == true)
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        Message = "Coupon already marked as used",
                        ErrorMessages = new string[] { "This coupon has already been used" }
                    });
                }

                var result = await _couponRepository.MarkCouponAsUsedAsync(id);

                // Invalidate user coupons cache
                //_cache.Remove($"user_coupons_{userCoupon.CustomerId}_false");
                //_cache.Remove($"user_coupons_{userCoupon.CustomerId}_true");

                return Ok(new ApiResponse<object>
                {
                    Message = "Coupon marked as used successfully",
                    Success = true,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while marking coupon as used");
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while marking coupon as used",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        #endregion
    }
}