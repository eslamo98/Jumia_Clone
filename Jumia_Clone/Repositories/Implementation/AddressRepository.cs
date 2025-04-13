using AutoMapper;
using Jumia_Clone.Data;
using Jumia_Clone.Models.DTOs.AddressDTOs;
using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Models.Entities;
using Jumia_Clone.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Jumia_Clone.Repositories.Implementation
{
    public class AddressRepository : IAddressRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<AddressRepository> _logger;

        public AddressRepository(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<AddressRepository> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<AddressDto>> GetAddressesByUserIdAsync(int userId, PaginationDto pagination)
        {
            try
            {
                // Start with the base query
                IQueryable<Address> query = _context.Addresses
                    .Where(a => a.UserId == userId);

                // Apply ordering
                query = query
                    .OrderByDescending(a => a.IsDefault)
                    .ThenBy(a => a.AddressName);

                // Apply pagination if needed
                if (pagination != null)
                {
                    query = query
                        .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                        .Take(pagination.PageSize);
                }

                // Execute the query
                var addresses = await query.ToListAsync();
                return _mapper.Map<IEnumerable<AddressDto>>(addresses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching addresses for user {UserId}", userId);
                throw;
            }
        }
        public async Task<AddressDto> GetAddressByIdAsync(int addressId)
        {
            try
            {
                var address = await _context.Addresses
                    .FirstOrDefaultAsync(a => a.AddressId == addressId );

                if (address == null)
                    return null;

                return _mapper.Map<AddressDto>(address);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching address {AddressId}", addressId);
                throw;
            }
        }

        public async Task<AddressDto> CreateAddressAsync(CreateAddressInputDto addressDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var address = _mapper.Map<Address>(addressDto);

                // If this is marked as default, unset any existing default addresses
                if (address.IsDefault == true)
                {
                    await UnsetDefaultAddressesAsync(address.UserId);
                }

                // If this is the first address, automatically set it as default
                var existingAddressCount = await _context.Addresses.CountAsync(a => a.UserId == address.UserId);
                if (existingAddressCount == 0)
                {
                    address.IsDefault = true;
                }

                _context.Addresses.Add(address);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return _mapper.Map<AddressDto>(address);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while creating address for user {UserId}", addressDto.UserId);
                throw;
            }
        }

        public async Task<AddressDto> UpdateAddressAsync(int addressId, UpdateAddressInputDto addressDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var address = await _context.Addresses
                    .FirstOrDefaultAsync(a => a.AddressId == addressId && a.UserId == addressDto.UserId);

                if (address == null)
                    throw new KeyNotFoundException($"Address with ID {addressId} not found for user {addressDto.UserId}");

                // Update properties
                _mapper.Map(addressDto, address);

                // If setting as default, unset other default addresses
                if (address.IsDefault == true)
                {
                    await UnsetDefaultAddressesAsync(address.UserId, addressId);
                }

                _context.Entry(address).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return _mapper.Map<AddressDto>(address);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while updating address {AddressId} for user {UserId}", addressId, addressDto.UserId);
                throw;
            }
        }

        public async Task<bool> DeleteAddressAsync(int addressId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var address = await _context.Addresses
                    .FirstOrDefaultAsync(a => a.AddressId == addressId);

                if (address == null)
                    return false;

                // If deleting a default address, set another one as default if available
                if (address.IsDefault == true)
                {
                    var nextAddress = await _context.Addresses
                        .Where(a => a.AddressId != addressId)
                        .OrderBy(a => a.AddressId)
                        .FirstOrDefaultAsync();

                    if (nextAddress != null)
                    {
                        nextAddress.IsDefault = true;
                        _context.Entry(nextAddress).State = EntityState.Modified;
                    }
                }

                _context.Addresses.Remove(address);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while deleting address {AddressId}", addressId);
                throw;
            }
        }

        public async Task<bool> AddressExistsAsync(int addressId)
        {
            return await _context.Addresses.AnyAsync(a => a.AddressId == addressId);
        }

        public async Task<int> GetAddressesCountByUserIdAsync(int userId)
        {
            return await _context.Addresses.CountAsync(a => a.UserId == userId);
        }

        // Helper method to unset default flag on all addresses except the specified one
        private async Task UnsetDefaultAddressesAsync(int userId, int? excludeAddressId = null)
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
}
