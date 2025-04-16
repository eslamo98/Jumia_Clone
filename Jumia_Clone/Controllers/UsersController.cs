using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Models.DTOs.UserDTOs;
using Jumia_Clone.Repositories.Interfaces;
using Jumia_Clone.Services.Interfaces;
using Jumia_Clone.Models.Enums;
using System.Security.Claims;
using AutoMapper;
using Jumia_Clone.Helpers;

namespace Jumia_Clone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IImageService _imageService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<UsersController> _logger;
        private readonly IMapper _mapper;

        public UsersController(
            IUserRepository userRepository,
            IImageService imageService,
            IMemoryCache cache,
            ILogger<UsersController> logger,
            IMapper mapper
            )
        {
            _userRepository = userRepository;
            _imageService = imageService;
            _cache = cache;
            _logger = logger;
            _mapper = mapper;
        }

        #region Helper Methods

        private int GetCurrentUserId()
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

        private void InvalidateUserCache(int userId, string userType = null)
        {
            var userCacheKey = $"user_{userId}";
            _cache.Remove(userCacheKey);

            if (!string.IsNullOrEmpty(userType))
            {
                if (userType.Equals("Customer", StringComparison.OrdinalIgnoreCase))
                {
                    _cache.Remove("customers_list");
                }
                else if (userType.Equals("Seller", StringComparison.OrdinalIgnoreCase))
                {
                    _cache.Remove("sellers_list");
                }
                else if (userType.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    _cache.Remove("admins_list");
                }
            }
        }

        #endregion

        #region General User Endpoints

        // GET: api/users/{id}
        [HttpGet("{id}")]
        //[Authorize]
        [EnableRateLimiting("standard")]
        [ResponseCache(Duration = 60)]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                // Check if user has access to this user's data
                if (!IsAdmin() && GetCurrentUserId() != id)
                {
                    return Forbid();
                }

                var cacheKey = $"user_{id}";

                // Try to get from cache first
                if (_cache.TryGetValue(cacheKey, out ApiResponse<UserDto> cachedResult))
                {
                    return Ok(cachedResult);
                }

                var user = await _userRepository.GetUserByIdAsync(id);

                if (user == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "User not found",
                        Success = false,
                        Data = null
                    });
                }

                var response = new ApiResponse<UserDto>
                {
                    Message = "User retrieved successfully",
                    Data = _mapper.Map<UserDto>(user),
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
                _logger.LogError(ex, "An error occurred while retrieving user {UserId}", id);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = $"An error occurred while retrieving user with id = {id}",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // PUT: api/users/{id}/change-password
        [HttpPut("{id}/change-password")]
        //[Authorize]
        [EnableRateLimiting("strict")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto changePasswordDto)
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
                // Check if user has access to change this password
                if (!IsAdmin() && GetCurrentUserId() != id)
                {
                    return Forbid();
                }

                var result = await _userRepository.ChangePasswordAsync(id,
                    changePasswordDto.CurrentPassword,
                    changePasswordDto.NewPassword);

                if (!result)
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        Message = "Password change failed",
                        ErrorMessages = new string[] { "Current password is incorrect" }
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Message = "Password changed successfully",
                    Success = true,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while changing password for user {UserId}", id);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while changing password",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // PATCH: api/users/{id}/status
        [HttpPatch("{id}/status")]
        //[Authorize(Roles = "Admin")]
        [EnableRateLimiting("strict")]
        public async Task<IActionResult> UpdateUserStatus(int id, [FromBody] UserStatusUpdateDto statusDto)
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
                var result = await _userRepository.UpdateUserStatusAsync(id, statusDto.IsActive);

                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "User not found",
                        Success = false,
                        Data = null
                    });
                }

                // Invalidate cache
                InvalidateUserCache(id);

                return Ok(new ApiResponse<object>
                {
                    Message = $"User status updated to {(statusDto.IsActive ? "active" : "inactive")} successfully",
                    Success = true,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating status for user {UserId}", id);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while updating user status",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        //[Authorize(Roles = "Admin")]
        [EnableRateLimiting("strict")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var result = await _userRepository.DeleteUserAsync(id);

                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "User not found",
                        Success = false,
                        Data = null
                    });
                }

                // Invalidate cache
                InvalidateUserCache(id);

                return Ok(new ApiResponse<object>
                {
                    Message = "User deleted successfully",
                    Success = true,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting user {UserId}", id);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while deleting user",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        #endregion

        #region Customer Endpoints

        // GET: api/users/customers
        [HttpGet("customers")]
        //[Authorize(Roles = "Admin")]
        [EnableRateLimiting("standard")]
        [ResponseCache(Duration = 60)]
        public async Task<IActionResult> GetAllCustomers([FromQuery] PaginationDto pagination, [FromQuery] string searchTerm = null)
        {
            try
            {
                // Create a cache key based on parameters
                var cacheKey = $"customers_list_{pagination.PageNumber}_{pagination.PageSize}_{searchTerm}";

                // Try to get from cache first
                if (_cache.TryGetValue(cacheKey, out ApiResponse<object> cachedResult))
                {
                    return Ok(cachedResult);
                }

                // Make sure pagination is valid
                if (pagination.PageNumber < 1)
                    pagination.PageNumber = 1;

                if (pagination.PageSize < 1)
                    pagination.PageSize = 10;

                var customers = await _userRepository.GetAllCustomersAsync(pagination, searchTerm);
                var totalCount = await _userRepository.GetCustomersCountAsync(searchTerm);

                var response = new ApiResponse<object>
                {
                    Message = "Successfully retrieved customers",
                    Data = new
                    {
                        Items = customers,
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

                _cache.Set(cacheKey, response, cacheEntryOptions);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving customers");
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving customers",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // GET: api/users/customers/{id}
        [HttpGet("customers/{id}")]
        //[Authorize]
        [EnableRateLimiting("standard")]
        [ResponseCache(Duration = 60)]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            try
            {
                var customer = await _userRepository.GetCustomerByIdAsync(id);

                if (customer == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Customer not found",
                        Success = false,
                        Data = null
                    });
                }

                // Check if user has access to this customer
                if (!IsAdmin() && GetCurrentUserId() != customer.UserId)
                {
                    return Forbid();
                }

                var cacheKey = $"customer_{id}";

                // Try to get from cache first
                if (_cache.TryGetValue(cacheKey, out ApiResponse<CustomerDto> cachedResult))
                {
                    return Ok(cachedResult);
                }

                var response = new ApiResponse<CustomerDto>
                {
                    Message = "Customer retrieved successfully",
                    Data = customer,
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
                _logger.LogError(ex, "An error occurred while retrieving customer {CustomerId}", id);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = $"An error occurred while retrieving customer with id = {id}",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // POST: api/users/customers/register
        [HttpPost("customers/register")]
        [AllowAnonymous]
        [EnableRateLimiting("strict")]
        public async Task<IActionResult> RegisterCustomer([FromForm] CustomerRegistrationDto registrationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Invalid registration data",
                    ErrorMessages = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToArray()
                });
            }

            try
            {
                // Check if email already exists
                if (await _userRepository.EmailExistsAsync(registrationDto.Email))
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        Message = "Registration failed",
                        ErrorMessages = new string[] { "Email is already in use" }
                    });
                }

                // Process profile image if provided
                string imagePath = null;
                if (registrationDto.ProfileImage != null)
                {
                    imagePath = await _imageService.SaveImageAsync(
                        registrationDto.ProfileImage, EntityType.User, StringHelper.GetUniquePath(registrationDto.FirstName));
                }

                // Register customer
                var customer = await _userRepository.RegisterCustomerAsync(registrationDto, imagePath);

                return CreatedAtAction(
                    nameof(GetCustomerById),
                    new { id = customer.CustomerId },
                    new ApiResponse<CustomerDto>
                    {
                        Message = "Customer registered successfully",
                        Data = customer,
                        Success = true
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while registering customer");
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while registering customer",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // PUT: api/users/customers/{id}
        [HttpPut("customers/{id}")]
        //[Authorize]
        [EnableRateLimiting("strict")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromForm] CustomerUpdateDto updateDto)
        {
            if (id != updateDto.UserId)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Invalid customer ID",
                    ErrorMessages = new string[] { "The customer ID in the URL does not match the ID in the body" }
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Invalid customer data",
                    ErrorMessages = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToArray()
                });
            }

            try
            {
                // Check if user has access to update this customer
                if (!IsAdmin() && GetCurrentUserId() != id)
                {
                    return Forbid();
                }

                // Get existing user to determine image path
                var existingUser = await _userRepository.GetUserByIdAsync(id);
                if (existingUser == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Customer not found",
                        Success = false,
                        Data = null
                    });
                }

                // Process profile image if provided
                string imagePath = existingUser.ProfileImageUrl;
                if (updateDto.ProfileImage != null)
                {
                    if (!string.IsNullOrEmpty(existingUser.ProfileImageUrl))
                    {
                        // Delete old image
                        await _imageService.DeleteImageAsync(existingUser.ProfileImageUrl);
                    }

                    // Save new image
                    imagePath = await _imageService.SaveImageAsync(
                        updateDto.ProfileImage, EntityType.User, StringHelper.GetUniquePath(existingUser.FirstName));
                }

                // Update customer
                var customer = await _userRepository.UpdateCustomerAsync(updateDto, imagePath);

                // Invalidate cache
                InvalidateUserCache(id, "Customer");

                return Ok(new ApiResponse<CustomerDto>
                {
                    Message = "Customer updated successfully",
                    Data = customer,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating customer {CustomerId}", id);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while updating the customer",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        #endregion

        #region Seller Endpoints

        // GET: api/users/sellers
        [HttpGet("sellers")]
        [EnableRateLimiting("standard")]
        [ResponseCache(Duration = 60)]
        public async Task<IActionResult> GetAllSellers(
            [FromQuery] PaginationDto pagination,
            [FromQuery] string searchTerm = null,
            [FromQuery] bool? isVerified = null)
        {
            try
            {
                // Create a cache key based on parameters
                var cacheKey = $"sellers_list_{pagination.PageNumber}_{pagination.PageSize}_{searchTerm}_{isVerified}";

                // Try to get from cache first
                if (_cache.TryGetValue(cacheKey, out ApiResponse<object> cachedResult))
                {
                    return Ok(cachedResult);
                }

                // Make sure pagination is valid
                if (pagination.PageNumber < 1)
                    pagination.PageNumber = 1;

                if (pagination.PageSize < 1)
                    pagination.PageSize = 10;

                var sellers = await _userRepository.GetAllSellersAsync(pagination, searchTerm, isVerified);
                var totalCount = await _userRepository.GetSellersCountAsync(searchTerm, isVerified);

                var response = new ApiResponse<object>
                {
                    Message = "Successfully retrieved sellers",
                    Data = new
                    {
                        Items = sellers,
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

                _cache.Set(cacheKey, response, cacheEntryOptions);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving sellers");
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving sellers",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // GET: api/users/sellers/{id}
        [HttpGet("sellers/{id}")]
        [EnableRateLimiting("standard")]
        [ResponseCache(Duration = 60)]
        public async Task<IActionResult> GetSellerById(int id)
        {
            try
            {
                var cacheKey = $"seller_{id}";

                // Try to get from cache first
                if (_cache.TryGetValue(cacheKey, out ApiResponse<SellerDto> cachedResult))
                {
                    return Ok(cachedResult);
                }

                var seller = await _userRepository.GetSellerByIdAsync(id);

                if (seller == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Seller not found",
                        Success = false,
                        Data = null
                    });
                }

                var response = new ApiResponse<SellerDto>
                {
                    Message = "Seller retrieved successfully",
                    Data = seller,
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
                _logger.LogError(ex, "An error occurred while retrieving seller {SellerId}", id);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = $"An error occurred while retrieving seller with id = {id}",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // POST: api/users/sellers/register
        [HttpPost("sellers/register")]
        [AllowAnonymous]
        [EnableRateLimiting("strict")]
        public async Task<IActionResult> RegisterSeller([FromForm] SellerRegistrationDto registrationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Invalid registration data",
                    ErrorMessages = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToArray()
                });
            }

            try
            {
                // Check if email already exists
                if (await _userRepository.EmailExistsAsync(registrationDto.Email))
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        Message = "Registration failed",
                        ErrorMessages = new string[] { "Email is already in use" }
                    });
                }

                // Process profile image if provided
                string profileImagePath = null;
                if (registrationDto.ProfileImage != null)
                {
                    profileImagePath = await _imageService.SaveImageAsync(
                        registrationDto.ProfileImage, EntityType.User, StringHelper.GetUniquePath(registrationDto.FirstName));
                }

                // Process business logo if provided
                string businessLogoPath = null;
                if (registrationDto.BusinessLogo != null)
                {
                    businessLogoPath = await _imageService.SaveImageAsync(
                        registrationDto.BusinessLogo, EntityType.User, $"{registrationDto.Email}_business");
                }

                // Register seller
                var seller = await _userRepository.RegisterSellerAsync(registrationDto, profileImagePath, businessLogoPath);

                return CreatedAtAction(
                    nameof(GetSellerById),
                    new { id = seller.SellerId },
                    new ApiResponse<SellerDto>
                    {
                        Message = "Seller registered successfully",
                        Data = seller,
                        Success = true
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while registering seller");
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while registering seller",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // PUT: api/users/sellers/{id}
        [HttpPut("sellers/{id}")]
        //[Authorize]
        [EnableRateLimiting("strict")]
        public async Task<IActionResult> UpdateSeller(int id, [FromForm] SellerUpdateDto updateDto)
        {
            if (id != updateDto.UserId)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Invalid seller ID",
                    ErrorMessages = new string[] { "The seller ID in the URL does not match the ID in the body" }
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Invalid seller data",
                    ErrorMessages = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToArray()
                });
            }

            try
            {
                // Check if user has access to update this seller
                if (!IsAdmin() && GetCurrentUserId() != id)
                {
                    return Forbid();
                }

                // Get existing user to determine image paths
                var existingUser = await _userRepository.GetUserByIdAsync(id);
                if (existingUser == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Seller not found",
                        Success = false,
                        Data = null
                    });
                }

                // Get existing seller details for business logo
                var existingSeller = await _userRepository.GetSellerByUserIdAsync(id);
                if (existingSeller == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Seller not found",
                        Success = false,
                        Data = null
                    });
                }

                // Process profile image if provided
                string profileImagePath = existingUser.ProfileImageUrl;
                if (updateDto.ProfileImage != null)
                {
                    if (!string.IsNullOrEmpty(existingUser.ProfileImageUrl))
                    {
                        // Delete old image
                        await _imageService.DeleteImageAsync(existingUser.ProfileImageUrl);
                    }

                    // Save new image
                    profileImagePath = await _imageService.SaveImageAsync(
                        updateDto.ProfileImage, EntityType.User, StringHelper.GetUniquePath(existingUser.FirstName));
                }

                // Process business logo if provided
                string businessLogoPath = existingSeller.BusinessLogo;
                if (updateDto.BusinessLogo != null)
                {
                    if (!string.IsNullOrEmpty(existingSeller.BusinessLogo))
                    {
                        // Delete old image
                        await _imageService.DeleteImageAsync(existingSeller.BusinessLogo);
                    }

                    // Save new image
                    businessLogoPath = await _imageService.SaveImageAsync(
                        updateDto.BusinessLogo, EntityType.User, StringHelper.GetUniquePath($"{existingUser.FirstName}_business"));
                }

                // Update seller
                var seller = await _userRepository.UpdateSellerAsync(updateDto, profileImagePath, businessLogoPath);

                // Invalidate cache
                InvalidateUserCache(id, "Seller");

                return Ok(new ApiResponse<SellerDto>
                {
                    Message = "Seller updated successfully",
                    Data = seller,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating seller {SellerId}", id);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while updating the seller",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // PATCH: api/users/sellers/{id}/verify
        [HttpPatch("sellers/{id}/verify")]
        //[Authorize(Roles = "Admin")]
        [EnableRateLimiting("strict")]
        public async Task<IActionResult> VerifySeller(int id, [FromBody] bool verify)
        {
            try
            {
                var seller = await _userRepository.VerifySellerAsync(id, verify);

                if (seller == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Seller not found",
                        Success = false,
                        Data = null
                    });
                }

                // Invalidate cache
                InvalidateUserCache(seller.UserId, "Seller");

                return Ok(new ApiResponse<SellerDto>
                {
                    Message = $"Seller {(verify ? "verified" : "unverified")} successfully",
                    Data = seller,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while verifying seller {SellerId}", id);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while verifying seller",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        #endregion

        #region Admin Endpoints

        // GET: api/users/admins
        [HttpGet("admins")]
        //[Authorize(Roles = "Admin")]
        [EnableRateLimiting("standard")]
        [ResponseCache(Duration = 60)]
        public async Task<IActionResult> GetAllAdmins([FromQuery] PaginationDto pagination, [FromQuery] string searchTerm = null)
        {
            try
            {
                // Create a cache key based on parameters
                var cacheKey = $"admins_list_{pagination.PageNumber}_{pagination.PageSize}_{searchTerm}";

                // Try to get from cache first
                if (_cache.TryGetValue(cacheKey, out ApiResponse<object> cachedResult))
                {
                    return Ok(cachedResult);
                }

                // Make sure pagination is valid
                if (pagination.PageNumber < 1)
                    pagination.PageNumber = 1;

                if (pagination.PageSize < 1)
                    pagination.PageSize = 10;

                var admins = await _userRepository.GetAllAdminsAsync(pagination, searchTerm);
                var totalCount = await _userRepository.GetAdminsCountAsync(searchTerm);

                var response = new ApiResponse<object>
                {
                    Message = "Successfully retrieved admins",
                    Data = new
                    {
                        Items = admins,
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

                _cache.Set(cacheKey, response, cacheEntryOptions);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving admins");
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving admins",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // GET: api/users/admins/{id}
        [HttpGet("admins/{id}")]
        //[Authorize(Roles = "Admin")]
        [EnableRateLimiting("standard")]
        [ResponseCache(Duration = 60)]
        public async Task<IActionResult> GetAdminById(int id)
        {
            try
            {
                var cacheKey = $"admin_{id}";

                // Try to get from cache first
                if (_cache.TryGetValue(cacheKey, out ApiResponse<AdminDto> cachedResult))
                {
                    return Ok(cachedResult);
                }

                var admin = await _userRepository.GetAdminByIdAsync(id);

                if (admin == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Admin not found",
                        Success = false,
                        Data = null
                    });
                }

                var response = new ApiResponse<AdminDto>
                {
                    Message = "Admin retrieved successfully",
                    Data = admin,
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
                _logger.LogError(ex, "An error occurred while retrieving admin {AdminId}", id);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = $"An error occurred while retrieving admin with id = {id}",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // POST: api/users/admins
        [HttpPost("admins")]
        //[Authorize(Roles = "Admin")]
        [EnableRateLimiting("strict")]
        public async Task<IActionResult> CreateAdmin([FromForm] AdminCreationDto creationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Invalid admin data",
                    ErrorMessages = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToArray()
                });
            }

            try
            {
                // Check if email already exists
                if (await _userRepository.EmailExistsAsync(creationDto.Email))
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        Message = "Admin creation failed",
                        ErrorMessages = new string[] { "Email is already in use" }
                    });
                }

                // Process profile image if provided
                string imagePath = null;
                if (creationDto.ProfileImage != null)
                {
                    imagePath = await _imageService.SaveImageAsync(
                        creationDto.ProfileImage, EntityType.User, StringHelper.GetUniquePath(creationDto.FirstName));
                }

                // Create admin
                var admin = await _userRepository.CreateAdminAsync(creationDto, imagePath);

                return CreatedAtAction(
                    nameof(GetAdminById),
                    new { id = admin.AdminId },
                    new ApiResponse<AdminDto>
                    {
                        Message = "Admin created successfully",
                        Data = admin,
                        Success = true
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating admin");
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while creating admin",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // PUT: api/users/admins/{id}
        [HttpPut("admins/{id}")]
        //[Authorize(Roles = "Admin")]
        [EnableRateLimiting("strict")]
        public async Task<IActionResult> UpdateAdmin(int id, [FromForm] AdminUpdateDto updateDto)
        {
            if (id != updateDto.UserId)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Invalid admin ID",
                    ErrorMessages = new string[] { "The admin ID in the URL does not match the ID in the body" }
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Invalid admin data",
                    ErrorMessages = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToArray()
                });
            }

            try
            {
                // Get existing user to determine image path
                var existingUser = await _userRepository.GetUserByIdAsync(id);
                if (existingUser == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Admin not found",
                        Success = false,
                        Data = null
                    });
                }

                // Process profile image if provided
                string imagePath = existingUser.ProfileImageUrl;
                if (updateDto.ProfileImage != null)
                {
                    if (!string.IsNullOrEmpty(existingUser.ProfileImageUrl))
                    {
                        // Delete old image
                        await _imageService.DeleteImageAsync(existingUser.ProfileImageUrl);
                    }

                    // Save new image
                    imagePath = await _imageService.SaveImageAsync(
                        updateDto.ProfileImage, EntityType.User, StringHelper.GetUniquePath(existingUser.FirstName));
                }

                // Update admin
                var admin = await _userRepository.UpdateAdminAsync(updateDto, imagePath);

                // Invalidate cache
                InvalidateUserCache(id, "Admin");

                return Ok(new ApiResponse<AdminDto>
                {
                    Message = "Admin updated successfully",
                    Data = admin,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating admin {AdminId}", id);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while updating the admin",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        #endregion
    }
}