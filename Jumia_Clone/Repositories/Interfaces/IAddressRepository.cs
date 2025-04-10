using Jumia_Clone.Data;
using Jumia_Clone.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Jumia_Clone.Repositories.Interfaces
{
    public interface IAddressRepository
    {
        Task<PagedResult<Address>> GetUserAddressesAsync(int userId, AddressFilterParameters filterParams);
        Task<Address> GetAddressByIdAsync(int addressId, int userId);
        Task<Address> AddAddressAsync(Address address);
        Task<Address> UpdateAddressAsync(Address address);
        Task<bool> DeleteAddressAsync(int addressId, int userId);
        Task<bool> AddressExistsAsync(int addressId, int userId);
    }

    public class AddressRepository : IAddressRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AddressRepository> _logger;

        public AddressRepository(ApplicationDbContext context, ILogger<AddressRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PagedResult<Address>> GetUserAddressesAsync(int userId, AddressFilterParameters filterParams)
        {
            // Start with base query
            var query = _context.Addresses
                .Where(a => a.UserId == userId);

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filterParams.SearchTerm))
            {
                query = query.Where(a =>
                    a.AddressName.Contains(filterParams.SearchTerm) ||
                    a.StreetAddress.Contains(filterParams.SearchTerm) ||
                    a.City.Contains(filterParams.SearchTerm) ||
                    a.State.Contains(filterParams.SearchTerm) ||
                    a.Country.Contains(filterParams.SearchTerm));
            }

            if (filterParams.IsDefault.HasValue)
            {
                query = query.Where(a => a.IsDefault == filterParams.IsDefault.Value);
            }

            if (!string.IsNullOrWhiteSpace(filterParams.Country))
            {
                query = query.Where(a => a.Country == filterParams.Country);
            }

            // Apply sorting
            query = filterParams.SortBy?.ToLower() switch
            {
                "name" => filterParams.SortOrder == SortOrder.Descending
                    ? query.OrderByDescending(a => a.AddressName)
                    : query.OrderBy(a => a.AddressName),
                "city" => filterParams.SortOrder == SortOrder.Descending
                    ? query.OrderByDescending(a => a.City)
                    : query.OrderBy(a => a.City),
                "country" => filterParams.SortOrder == SortOrder.Descending
                    ? query.OrderByDescending(a => a.Country)
                    : query.OrderBy(a => a.Country),
                _ => query.OrderByDescending(a => a.IsDefault).ThenBy(a => a.AddressName)
            };

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var items = await query
                .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
                .Take(filterParams.PageSize)
                .ToListAsync();

            return new PagedResult<Address>(
                items,
                totalCount,
                filterParams.PageNumber,
                filterParams.PageSize);
        }

        public async Task<Address> GetAddressByIdAsync(int addressId, int userId)
        {
            return await _context.Addresses
                .FirstOrDefaultAsync(a => a.AddressId == addressId && a.UserId == userId);
        }

        public async Task<Address> AddAddressAsync(Address address)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // If setting as default, unset all other default addresses for this user
                if (address.IsDefault == true)
                {
                    await UnsetDefaultAddressesAsync(address.UserId, excludeAddressId: null);
                }

                // If this is the first address for the user, make it default regardless of IsDefault value
                if (!await _context.Addresses.AnyAsync(a => a.UserId == address.UserId))
                {
                    address.IsDefault = true;
                }

                _context.Addresses.Add(address);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return address;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while adding address for user {UserId}", address.UserId);
                throw;
            }
        }

        public async Task<Address> UpdateAddressAsync(Address address)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // If setting as default, unset all other default addresses for this user
                if (address.IsDefault == true)
                {
                    await UnsetDefaultAddressesAsync(address.UserId, address.AddressId);
                }

                _context.Entry(address).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return address;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while updating address {AddressId} for user {UserId}",
                    address.AddressId, address.UserId);
                throw;
            }
        }

        public async Task<bool> DeleteAddressAsync(int addressId, int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var address = await _context.Addresses
                    .FirstOrDefaultAsync(a => a.AddressId == addressId && a.UserId == userId);

                if (address == null)
                {
                    return false;
                }

                // If deleting default address, set another address as default if any exists
                if (address.IsDefault == true)
                {
                    var nextAddress = await _context.Addresses
                        .Where(a => a.UserId == userId && a.AddressId != addressId)
                        .FirstOrDefaultAsync();

                    if (nextAddress != null)
                    {
                        nextAddress.IsDefault = true;
                        _context.Entry(nextAddress).State = EntityState.Modified;
                    }
                }

                // Hard delete
                _context.Addresses.Remove(address);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while deleting address {AddressId} for user {UserId}",
                    addressId, userId);
                throw;
            }
        }

        public async Task<bool> AddressExistsAsync(int addressId, int userId)
        {
            return await _context.Addresses
                .AnyAsync(a => a.AddressId == addressId && a.UserId == userId);
        }

        // Helper method to unset default status on all addresses except the one being modified
        private async Task UnsetDefaultAddressesAsync(int userId, int? excludeAddressId)
        {
            var defaultAddresses = await _context.Addresses
                .Where(a => a.UserId == userId && a.IsDefault == true)
                .ToListAsync();

            foreach (var defaultAddress in defaultAddresses)
            {
                if (excludeAddressId == null || defaultAddress.AddressId != excludeAddressId)
                {
                    defaultAddress.IsDefault = false;
                    _context.Entry(defaultAddress).State = EntityState.Modified;
                }
            }
        }
    }

    // Your existing pagination and filtering helpers remain unchanged
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; }
        public int TotalCount { get; }
        public int PageNumber { get; }
        public int PageSize { get; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public PagedResult(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }

    public class AddressFilterParameters
    {
        private const int MaxPageSize = 50;
        private int _pageSize = 10;

        public int PageNumber { get; set; } = 1;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }

        public string SearchTerm { get; set; }
        public bool? IsDefault { get; set; }
        public string Country { get; set; }
        public string SortBy { get; set; }
        public SortOrder SortOrder { get; set; } = SortOrder.Ascending;
    }

    public enum SortOrder
    {
        Ascending,
        Descending
    }
}
