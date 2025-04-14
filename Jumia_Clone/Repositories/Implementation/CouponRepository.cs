using AutoMapper;
using Jumia_Clone.Data;
using Jumia_Clone.Models.DTOs.CouponDTOs;
using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Models.Entities;
using Jumia_Clone.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Jumia_Clone.Repositories.Implementation
{
    public class CouponRepository : ICouponRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CouponRepository> _logger;

        public CouponRepository(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<CouponRepository> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        #region Coupon CRUD operations

        public async Task<IEnumerable<CouponDto>> GetAllCouponsAsync(PaginationDto pagination, bool? isActive = null, string searchTerm = null)
        {
            try
            {
                IQueryable<Coupon> query = _context.Coupons;

                // Apply filters
                if (isActive.HasValue)
                {
                    query = query.Where(c => c.IsActive == isActive.Value);
                }

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    query = query.Where(c =>
                        c.Code.ToLower().Contains(searchTerm) ||
                        (c.Description != null && c.Description.ToLower().Contains(searchTerm)));
                }

                // Apply order by
                query = query.OrderByDescending(c => c.StartDate);

                // Apply pagination
                if (pagination != null)
                {
                    query = query.Skip((pagination.PageNumber - 1) * pagination.PageSize)
                              .Take(pagination.PageSize);
                }

                var coupons = await query.ToListAsync();
                return _mapper.Map<IEnumerable<CouponDto>>(coupons);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all coupons");
                throw;
            }
        }

        public async Task<int> GetCouponsCountAsync(bool? isActive = null, string searchTerm = null)
        {
            try
            {
                IQueryable<Coupon> query = _context.Coupons;

                // Apply filters
                if (isActive.HasValue)
                {
                    query = query.Where(c => c.IsActive == isActive.Value);
                }

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    query = query.Where(c =>
                        c.Code.ToLower().Contains(searchTerm) ||
                        (c.Description != null && c.Description.ToLower().Contains(searchTerm)));
                }

                return await query.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while counting coupons");
                throw;
            }
        }

        public async Task<CouponDto> GetCouponByIdAsync(int couponId)
        {
            try
            {
                var coupon = await _context.Coupons
                    .FirstOrDefaultAsync(c => c.CouponId == couponId);

                if (coupon == null)
                    return null;

                return _mapper.Map<CouponDto>(coupon);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching coupon with ID {CouponId}", couponId);
                throw;
            }
        }

        public async Task<CouponDto> GetCouponByCodeAsync(string code)
        {
            try
            {
                var coupon = await _context.Coupons
                    .FirstOrDefaultAsync(c => c.Code == code);

                if (coupon == null)
                    return null;

                return _mapper.Map<CouponDto>(coupon);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching coupon with code {CouponCode}", code);
                throw;
            }
        }

        public async Task<CouponDto> CreateCouponAsync(CreateCouponDto couponDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validate coupon code uniqueness
                if (await _context.Coupons.AnyAsync(c => c.Code == couponDto.Code))
                {
                    throw new InvalidOperationException($"Coupon code '{couponDto.Code}' already exists");
                }

                // Map DTO to entity
                var coupon = _mapper.Map<Coupon>(couponDto);

                // Initialize usage count
                coupon.UsageCount = 0;

                _context.Coupons.Add(coupon);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return _mapper.Map<CouponDto>(coupon);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while creating coupon");
                throw;
            }
        }

        public async Task<CouponDto> UpdateCouponAsync(UpdateCouponDto couponDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var coupon = await _context.Coupons
                    .FirstOrDefaultAsync(c => c.CouponId == couponDto.CouponId);

                if (coupon == null)
                {
                    throw new KeyNotFoundException($"Coupon with ID {couponDto.CouponId} not found");
                }

                // Update properties if provided
                if (!string.IsNullOrWhiteSpace(couponDto.Description))
                {
                    coupon.Description = couponDto.Description;
                }

                if (couponDto.DiscountAmount.HasValue)
                {
                    coupon.DiscountAmount = couponDto.DiscountAmount.Value;
                }

                if (couponDto.MinimumPurchase.HasValue)
                {
                    coupon.MinimumPurchase = couponDto.MinimumPurchase.Value;
                }

                if (!string.IsNullOrWhiteSpace(couponDto.DiscountType))
                {
                    coupon.DiscountType = couponDto.DiscountType;
                }

                if (couponDto.StartDate.HasValue)
                {
                    coupon.StartDate = couponDto.StartDate.Value;
                }

                if (couponDto.EndDate.HasValue)
                {
                    coupon.EndDate = couponDto.EndDate.Value;
                }

                if (couponDto.IsActive.HasValue)
                {
                    coupon.IsActive = couponDto.IsActive.Value;
                }

                if (couponDto.UsageLimit.HasValue)
                {
                    coupon.UsageLimit = couponDto.UsageLimit.Value;
                }

                _context.Entry(coupon).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return _mapper.Map<CouponDto>(coupon);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while updating coupon with ID {CouponId}", couponDto.CouponId);
                throw;
            }
        }

        public async Task<bool> DeleteCouponAsync(int couponId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var coupon = await _context.Coupons
                    .FirstOrDefaultAsync(c => c.CouponId == couponId);

                if (coupon == null)
                    return false;

                // Check if the coupon has already been used in orders
                bool isUsedInOrders = await _context.Orders.AnyAsync(o => o.CouponId == couponId);

                if (isUsedInOrders)
                {
                    // If used in orders, just mark it as inactive instead of deleting
                    coupon.IsActive = false;
                    _context.Entry(coupon).State = EntityState.Modified;
                }
                else
                {
                    // If not used in orders, we can safely delete it
                    // First, delete related UserCoupons
                    var userCoupons = await _context.UserCoupons
                        .Where(uc => uc.CouponId == couponId)
                        .ToListAsync();

                    _context.UserCoupons.RemoveRange(userCoupons);

                    // Then delete the coupon
                    _context.Coupons.Remove(coupon);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while deleting coupon with ID {CouponId}", couponId);
                throw;
            }
        }

        public async Task<bool> CouponExistsAsync(int couponId)
        {
            try
            {
                return await _context.Coupons.AnyAsync(c => c.CouponId == couponId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if coupon exists with ID {CouponId}", couponId);
                throw;
            }
        }

        public async Task<bool> CouponCodeExistsAsync(string code)
        {
            try
            {
                return await _context.Coupons.AnyAsync(c => c.Code == code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if coupon code exists {CouponCode}", code);
                throw;
            }
        }

        #endregion

        #region Coupon Validation and Usage

        public async Task<CouponValidationResultDto> ValidateCouponAsync(ValidateCouponDto validateDto)
        {
            try
            {
                // Get the coupon by code
                var coupon = await _context.Coupons
                    .FirstOrDefaultAsync(c => c.Code == validateDto.CouponCode);

                if (coupon == null)
                {
                    return new CouponValidationResultDto
                    {
                        IsValid = false,
                        Message = "Coupon code not found",
                        DiscountValue = 0
                    };
                }

                // Check if coupon is active
                if (coupon.IsActive != true)
                {
                    return new CouponValidationResultDto
                    {
                        IsValid = false,
                        Message = "Coupon is not active",
                        Coupon = _mapper.Map<CouponDto>(coupon),
                        DiscountValue = 0
                    };
                }

                // Check if coupon is within valid date range
                var currentDate = DateTime.UtcNow;
                if (currentDate < coupon.StartDate || currentDate > coupon.EndDate)
                {
                    return new CouponValidationResultDto
                    {
                        IsValid = false,
                        Message = "Coupon is not valid at this time",
                        Coupon = _mapper.Map<CouponDto>(coupon),
                        DiscountValue = 0
                    };
                }

                // Check minimum purchase requirement
                if (coupon.MinimumPurchase.HasValue && validateDto.CartTotal < coupon.MinimumPurchase.Value)
                {
                    return new CouponValidationResultDto
                    {
                        IsValid = false,
                        Message = $"Minimum purchase of {coupon.MinimumPurchase.Value:C} required",
                        Coupon = _mapper.Map<CouponDto>(coupon),
                        DiscountValue = 0
                    };
                }

                // Check usage limit
                if (coupon.UsageLimit.HasValue && coupon.UsageCount >= coupon.UsageLimit.Value)
                {
                    return new CouponValidationResultDto
                    {
                        IsValid = false,
                        Message = "Coupon usage limit has been reached",
                        Coupon = _mapper.Map<CouponDto>(coupon),
                        DiscountValue = 0
                    };
                }

                // Check if user has already used this coupon
                bool alreadyUsed = await _context.UserCoupons
                    .AnyAsync(uc => uc.CouponId == coupon.CouponId && uc.CustomerId == validateDto.CustomerId && uc.IsUsed == true);

                if (alreadyUsed)
                {
                    return new CouponValidationResultDto
                    {
                        IsValid = false,
                        Message = "You have already used this coupon",
                        Coupon = _mapper.Map<CouponDto>(coupon),
                        DiscountValue = 0
                    };
                }

                // Calculate discount value
                decimal discountValue = 0;
                if (coupon.DiscountType == "Fixed")
                {
                    discountValue = coupon.DiscountAmount;
                }
                else if (coupon.DiscountType == "Percentage")
                {
                    discountValue = validateDto.CartTotal * (coupon.DiscountAmount / 100);
                }

                // Coupon is valid
                return new CouponValidationResultDto
                {
                    IsValid = true,
                    Message = "Coupon is valid",
                    Coupon = _mapper.Map<CouponDto>(coupon),
                    DiscountValue = discountValue
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while validating coupon code {CouponCode}", validateDto.CouponCode);
                throw;
            }
        }

        public async Task<bool> IncrementCouponUsageAsync(int couponId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var coupon = await _context.Coupons
                    .FirstOrDefaultAsync(c => c.CouponId == couponId);

                if (coupon == null)
                    return false;

                // Increment usage count
                coupon.UsageCount = (coupon.UsageCount ?? 0) + 1;

                // If usage limit reached, deactivate the coupon
                if (coupon.UsageLimit.HasValue && coupon.UsageCount >= coupon.UsageLimit.Value)
                {
                    coupon.IsActive = false;
                }

                _context.Entry(coupon).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while incrementing usage count for coupon {CouponId}", couponId);
                throw;
            }
        }

        #endregion

        #region User Coupon Operations

        public async Task<UserCouponDto> AssignCouponToUserAsync(AssignCouponDto assignDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Check if the coupon exists
                var coupon = await _context.Coupons
                    .FirstOrDefaultAsync(c => c.CouponId == assignDto.CouponId);

                if (coupon == null)
                {
                    throw new KeyNotFoundException($"Coupon with ID {assignDto.CouponId} not found");
                }

                // Check if the customer exists
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.CustomerId == assignDto.CustomerId);

                if (customer == null)
                {
                    throw new KeyNotFoundException($"Customer with ID {assignDto.CustomerId} not found");
                }

                // Check if the user already has this coupon
                var existingUserCoupon = await _context.UserCoupons
                    .FirstOrDefaultAsync(uc => uc.CouponId == assignDto.CouponId && uc.CustomerId == assignDto.CustomerId);

                if (existingUserCoupon != null)
                {
                    // If already assigned but used, can reassign it
                    if (existingUserCoupon.IsUsed == true)
                    {
                        existingUserCoupon.IsUsed = false;
                        existingUserCoupon.AssignedAt = DateTime.UtcNow;
                        existingUserCoupon.UsedAt = null;

                        _context.Entry(existingUserCoupon).State = EntityState.Modified;
                        await _context.SaveChangesAsync();

                        var result = _mapper.Map<UserCouponDto>(existingUserCoupon);
                        result.Coupon = _mapper.Map<CouponDto>(coupon);

                        await transaction.CommitAsync();
                        return result;
                    }
                    else
                    {
                        throw new InvalidOperationException("Customer already has this coupon assigned");
                    }
                }

                // Create new user coupon
                var userCoupon = new UserCoupon
                {
                    CustomerId = assignDto.CustomerId,
                    CouponId = assignDto.CouponId,
                    IsUsed = false,
                    AssignedAt = DateTime.UtcNow
                };

                _context.UserCoupons.Add(userCoupon);
                await _context.SaveChangesAsync();

                var userCouponDto = _mapper.Map<UserCouponDto>(userCoupon);
                userCouponDto.Coupon = _mapper.Map<CouponDto>(coupon);

                await transaction.CommitAsync();
                return userCouponDto;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while assigning coupon {CouponId} to customer {CustomerId}",
                    assignDto.CouponId, assignDto.CustomerId);
                throw;
            }
        }

        public async Task<IEnumerable<UserCouponDto>> GetUserCouponsAsync(int customerId, bool includeUsed = false)
        {
            try
            {
                IQueryable<UserCoupon> query = _context.UserCoupons
                    .Include(uc => uc.Coupon)
                    .Where(uc => uc.CustomerId == customerId);

                if (!includeUsed)
                {
                    query = query.Where(uc => uc.IsUsed != true);
                }

                var userCoupons = await query.ToListAsync();

                return userCoupons.Select(uc => {
                    var dto = _mapper.Map<UserCouponDto>(uc);
                    dto.Coupon = _mapper.Map<CouponDto>(uc.Coupon);
                    return dto;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching coupons for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<bool> MarkCouponAsUsedAsync(int userCouponId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var userCoupon = await _context.UserCoupons
                    .FirstOrDefaultAsync(uc => uc.UserCouponId == userCouponId);

                if (userCoupon == null)
                    return false;

                if (userCoupon.IsUsed == true)
                    return true; // Already marked as used

                userCoupon.IsUsed = true;
                userCoupon.UsedAt = DateTime.UtcNow;

                _context.Entry(userCoupon).State = EntityState.Modified;

                // Increment coupon usage count
                await IncrementCouponUsageAsync(userCoupon.CouponId);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while marking user coupon {UserCouponId} as used", userCouponId);
                throw;
            }
        }

        public async Task<bool> UserHasCouponAsync(int customerId, int couponId)
        {
            try
            {
                return await _context.UserCoupons
                    .AnyAsync(uc => uc.CustomerId == customerId && uc.CouponId == couponId && uc.IsUsed != true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if customer {CustomerId} has coupon {CouponId}",
                    customerId, couponId);
                throw;
            }
        }

        public async Task<UserCouponDto> GetUserCouponByIdAsync(int userCouponId)
        {
            try
            {
                var userCoupon = await _context.UserCoupons
                    .Include(uc => uc.Coupon)
                    .FirstOrDefaultAsync(uc => uc.UserCouponId == userCouponId);

                if (userCoupon == null)
                    return null;

                var userCouponDto = _mapper.Map<UserCouponDto>(userCoupon);
                userCouponDto.Coupon = _mapper.Map<CouponDto>(userCoupon.Coupon);

                return userCouponDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching user coupon {UserCouponId}", userCouponId);
                throw;
            }
        }

        #endregion
    }
}
