using Jumia_Clone.Models.DTOs.AddressDTO;
using Jumia_Clone.Models.Entities;
using Jumia_Clone.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using AutoMapper;

namespace Jumia_Clone.Controllers
{
    [Route("api/users/addresses")]
    [ApiController]
    [Authorize]
    [ServiceFilter(typeof(ValidateModelStateFilter))]
    public class UserAddressesController : ControllerBase
    {
        private readonly IAddressRepository _addressRepository;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        private readonly ILogger<UserAddressesController> _logger;

        public UserAddressesController(
            IAddressRepository addressRepository,
            IMapper mapper,
            IMemoryCache cache,
            ILogger<UserAddressesController> logger)
        {
            _addressRepository = addressRepository;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
        }

        [HttpGet]
        [EnableRateLimiting("standard")]
        [ResponseCache(Duration = 60)]
        public async Task<ActionResult<PagedAddressesDto>> GetUserAddresses([FromQuery] AddressFilterRequest filterRequest)
        {
            try
            {
                var userId = GetCurrentUserId();
                var cacheKey = $"addresses_{userId}_{JsonSerializer.Serialize(filterRequest)}";

                if (_cache.TryGetValue(cacheKey, out PagedAddressesDto cachedResult))
                    return Ok(cachedResult);

                var filterParams = _mapper.Map<AddressFilterParameters>(filterRequest);
                if (!string.IsNullOrEmpty(filterRequest.SortOrder))
                {
                    filterParams.SortOrder = filterRequest.SortOrder.ToLower() == "desc"
                        ? SortOrder.Descending
                        : SortOrder.Ascending;
                }

                var pagedResult = await _addressRepository.GetUserAddressesAsync(userId, filterParams);
                var pagedDto = new PagedAddressesDto
                {
                    Items = _mapper.Map<IEnumerable<AddressDto>>(pagedResult.Items),
                    TotalCount = pagedResult.TotalCount,
                    PageNumber = pagedResult.PageNumber,
                    PageSize = pagedResult.PageSize,
                    TotalPages = pagedResult.TotalPages,
                    HasPreviousPage = pagedResult.HasPreviousPage,
                    HasNextPage = pagedResult.HasNextPage
                };

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(1))
                    .SetSlidingExpiration(TimeSpan.FromSeconds(30));
                _cache.Set(cacheKey, pagedDto, cacheOptions);

                return Ok(pagedDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving addresses for user");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        [HttpGet("{id}")]
        [EnableRateLimiting("standard")]
        [ResponseCache(Duration = 60)]
        public async Task<ActionResult<AddressDto>> GetAddress(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var cacheKey = $"address_{userId}_{id}";

                if (_cache.TryGetValue(cacheKey, out AddressDto cachedAddress))
                    return Ok(cachedAddress);

                var address = await _addressRepository.GetAddressByIdAsync(id, 1);
                if (address == null)
                    return NotFound();

                var addressDto = _mapper.Map<AddressDto>(address);

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(1))
                    .SetSlidingExpiration(TimeSpan.FromSeconds(30));
                _cache.Set(cacheKey, addressDto, cacheOptions);

                return Ok(addressDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving address {AddressId}", id);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        [HttpPost]
        [EnableRateLimiting("strict")]
        public async Task<ActionResult<AddressDto>> CreateAddress(CreateAddressRequest createRequest)
        {
            try
            {
                var userId = GetCurrentUserId();
                var address = _mapper.Map<Address>(createRequest);
                address.UserId = userId;

                var createdAddress = await _addressRepository.AddAddressAsync(address);
                InvalidateUserAddressesCache(userId);

                var addressDto = _mapper.Map<AddressDto>(createdAddress);

                return CreatedAtAction(nameof(GetAddress), new { id = createdAddress.AddressId }, addressDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating address");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        [HttpPut("{id}")]
        [EnableRateLimiting("strict")]
        public async Task<IActionResult> UpdateAddress(int id, UpdateAddressRequest updateRequest)
        {
            try
            {
                var userId = GetCurrentUserId();
                var existingAddress = await _addressRepository.GetAddressByIdAsync(id, 1);

                if (existingAddress == null)
                    return NotFound();

                _mapper.Map(updateRequest, existingAddress);
                existingAddress.AddressId = id;
                existingAddress.UserId = userId;

                await _addressRepository.UpdateAddressAsync(existingAddress);
                InvalidateUserAddressesCache(userId);
                InvalidateAddressCache(userId, id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address {AddressId}", id);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        [HttpDelete("{id}")]
        [EnableRateLimiting("strict")]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _addressRepository.DeleteAddressAsync(id, userId);

                if (!result)
                    return NotFound();

                InvalidateUserAddressesCache(userId);
                InvalidateAddressCache(userId, id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting address {AddressId}", id);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        private int GetCurrentUserId()
        {
            //var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            //    throw new UnauthorizedAccessException("User ID not found in the claims");

            return 1;
        }

        private void InvalidateUserAddressesCache(int userId)
        {
            var keys = _cache.GetKeys()
                .Where(k => k.ToString().StartsWith($"addresses_{userId}_"))
                .ToList();

            foreach (var key in keys)
                _cache.Remove(key);
        }

        private void InvalidateAddressCache(int userId, int addressId)
        {
            var cacheKey = $"address_{userId}_{addressId}";
            _cache.Remove(cacheKey);
        }
    }

    public static class MemoryCacheExtensions
    {
        public static IEnumerable<object> GetKeys(this IMemoryCache memoryCache)
        {
            var entriesCollectionProperty = typeof(MemoryCache).GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance);
            var entriesCollection = entriesCollectionProperty?.GetValue(memoryCache) as dynamic;

            var keys = new List<object>();
            if (entriesCollection != null)
            {
                foreach (var entry in entriesCollection)
                {
                    var keyProperty = entry.GetType().GetProperty("Key");
                    var key = keyProperty?.GetValue(entry);
                    if (key != null)
                        keys.Add(key);
                }
            }

            return keys;
        }
    }

    public class ValidateModelStateFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(new
                {
                    errors = context.ModelState
                        .Where(e => e.Value.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        )
                });
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
