using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using AutoMapper;
using Jumia_Clone.Models.DTOs.AddressDTOs;
using Microsoft.AspNetCore.Authorization;

namespace Jumia_Clone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AddressesController : ControllerBase
    {
        private readonly IAddressRepository _addressRepository;
        private readonly IMapper _mapper;
        //private readonly IMemoryCache _cache;
        private readonly ILogger<AddressesController> _logger;

        public AddressesController(
            IAddressRepository addressRepository,
            IMapper mapper,
            IMemoryCache cache,
            ILogger<AddressesController> logger)
        {
            _addressRepository = addressRepository;
            _mapper = mapper;
            //_cache = cache;
            _logger = logger;
        }

        // GET: api/addresses
        [HttpGet]
        [EnableRateLimiting("standard")]
        [ResponseCache(Duration = 60)]
        public async Task<IActionResult> GetAll([FromQuery] PaginationDto pagination, [FromQuery] int userId)
        {
            try
            {
                // Create a cache key based on parameters
                var cacheKey = $"addresses_{userId}_{pagination.PageNumber}_{pagination.PageSize}";

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

                var addresses = await _addressRepository.GetAddressesByUserIdAsync(userId, pagination);
                var totalCount = await _addressRepository.GetAddressesCountByUserIdAsync(userId);

                var response = new ApiResponse<object>
                {
                    Message = "Successfully retrieved addresses",
                    Data = new
                    {
                        Items = addresses,
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
                _logger.LogError(ex, "An error occurred while retrieving addresses for user {UserId}", userId);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving addresses",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // GET: api/addresses/{id}
        [HttpGet("{id}")]
        [EnableRateLimiting("standard")]
        [ResponseCache(Duration = 60)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                // Create cache key
                var cacheKey = $"address_{id}";

                // Try to get from cache first
                //if (_cache.TryGetValue(cacheKey, out ApiResponse<AddressDto> cachedResult))
                //{
                //    return Ok(cachedResult);
                //}

                var address = await _addressRepository.GetAddressByIdAsync(id);

                if (address == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Address not found",
                        Success = false,
                        Data = null
                    });
                }

                var response = new ApiResponse<AddressDto>
                {
                    Message = "Address retrieved successfully",
                    Data = address,
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
                _logger.LogError(ex, "An error occurred while retrieving address {AddressId}", id);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = $"An error occurred while retrieving address with id = {id}",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // POST: api/addresses
        [HttpPost]
        [EnableRateLimiting("strict")]
        public async Task<IActionResult> Create([FromBody] CreateAddressInputDto addressDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Invalid address data",
                    ErrorMessages = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToArray()
                });
            }

            try
            {
                var createdAddress = await _addressRepository.CreateAddressAsync(addressDto);

                // Invalidate cache for user's addresses
                //InvalidateUserAddressesCache(addressDto.UserId);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = createdAddress.AddressId, userId = addressDto.UserId },
                    new ApiResponse<AddressDto>
                    {
                        Message = "Address created successfully",
                        Data = createdAddress,
                        Success = true
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating address for user {UserId}", addressDto.UserId);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while creating the address",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // PUT: api/addresses/{id}
        [HttpPut("{id}")]
        [EnableRateLimiting("strict")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAddressInputDto addressDto)
        {
            if (id != addressDto.AddressId)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Invalid address ID",
                    ErrorMessages = new string[] { "The address ID in the URL does not match the ID in the body" }
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Invalid address data",
                    ErrorMessages = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToArray()
                });
            }

            try
            {
                if (!await _addressRepository.AddressExistsAsync(id))
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Address not found",
                        Success = false,
                        Data = null
                    });
                }

                var updatedAddress = await _addressRepository.UpdateAddressAsync(id, addressDto);

                // Invalidate caches
                //InvalidateUserAddressesCache(addressDto.UserId);
                //InvalidateAddressCache(id);

                return Ok(new ApiResponse<AddressDto>
                {
                    Message = "Address updated successfully",
                    Data = updatedAddress,
                    Success = true
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ApiResponse<object>
                {
                    Message = "Address not found",
                    Success = false,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating address {AddressId} for user {UserId}", id, addressDto.UserId);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while updating the address",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // DELETE: api/addresses/{id}
        [HttpDelete("{id}")]
        [EnableRateLimiting("strict")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (!await _addressRepository.AddressExistsAsync(id))
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Address not found",
                        Success = false,
                        Data = null
                    });
                }

                var result = await _addressRepository.DeleteAddressAsync(id);

                // Invalidate caches
                //InvalidateAddressCache(id);

                return Ok(new ApiResponse<object>
                {
                    Message = "Address deleted successfully",
                    Data = null,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting address {AddressId}", id);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while deleting the address",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // Cache invalidation helpers
        ////private void InvalidateUserAddressesCache(int userId)
        //{
        //    // Generic approach that doesn't require listing all keys
        //    var cacheKeys = new List<string>();

        //    // We know the pattern of our cache keys, so we can remove specific ones
        //    // For addresses list we add a simple placeholder that matches all page numbers and sizes
        //    var listCacheKeyPattern = $"addresses_{userId}_";

        //    // Remove all entries from cache that start with this pattern
        //    //_cache.Remove(listCacheKeyPattern);
        //}

        ////private void InvalidateAddressCache(int addressId)
        //{
        //    var cacheKey = $"address_{addressId}";
        //    //_cache.Remove(cacheKey);
        //}
    }
}