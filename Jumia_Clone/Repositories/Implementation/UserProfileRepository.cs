using Jumia_Clone.Data;
using Jumia_Clone.Models.DTOs.AuthenticationDTOs;
using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Models.Entities;
using Jumia_Clone.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Jumia_Clone.Repositories.Implementation
{
    public class UserProfileRepository : IUserProfileRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public UserProfileRepository(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<ApiResponse<UserResponseDto>> GetUserByIdAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return new ApiResponse<UserResponseDto>(null, "User not found", false);

            var dto = new UserResponseDto
            {
                Id = user.UserId,
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                UserType = user.UserType
            };
            return new ApiResponse<UserResponseDto>(dto, "User retrieved successfully", true);
        }

        public async Task<ApiResponse<List<UserResponseDto>>> GetAllUsersAsync()
        {
            var users = await _context.Users.ToListAsync();
            return new ApiResponse<List<UserResponseDto>>(
                users.Select(u => new UserResponseDto
                {
                    Id = u.UserId,
                    FullName = $"{u.FirstName} {u.LastName}",
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    UserType = u.UserType
                }).ToList(),
                "Users retrieved successfully",
                true);
        }

        public async Task<ApiResponse<UserResponseDto>> CreateUserAsync(UpdateUserProfileDto userDto)
        {
            var user = new User
            {
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email,
                PhoneNumber = userDto.PhoneNumber,
                PasswordHash = _userManager.PasswordHasher.HashPassword(null, "DefaultPassword123!"),
                UserType = "customer",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var dto = new UserResponseDto
            {
                Id = user.UserId,
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                UserType = user.UserType
            };
            return new ApiResponse<UserResponseDto>(dto, "User created successfully. Please set a password using the change-password endpoint.", true);
        }

        public async Task<ApiResponse<UserResponseDto>> UpdateUserAsync(int id, UpdateUserProfileDto userDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return new ApiResponse<UserResponseDto>(null, "User not found", false);

            user.FirstName = userDto.FirstName;
            user.LastName = userDto.LastName;
            user.Email = userDto.Email;
            user.PhoneNumber = userDto.PhoneNumber;
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            var dto = new UserResponseDto
            {
                Id = user.UserId,
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                UserType = user.UserType
            };
            return new ApiResponse<UserResponseDto>(dto, "User updated successfully", true);
        }

        public async Task<ApiResponse<string>> DeleteUserAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Load user with direct dependencies
                var user = await _context.Users
                    .Include(u => u.Addresses)
                    .Include(u => u.Admin)
                    .Include(u => u.Affiliates)
                    .Include(u => u.Customer)
                    .Include(u => u.Seller)
                    .FirstOrDefaultAsync(u => u.UserId == id);

                if (user == null)
                    return new ApiResponse<string>(null, "User not found", false);

                // Check for critical data before proceeding
                if (user.Customer != null)
                {
                    var activeOrders = await _context.Orders
                        .Where(o => o.CustomerId == user.Customer.CustomerId && o.PaymentStatus != "completed")
                        .AnyAsync();
                    if (activeOrders)
                        return new ApiResponse<string>(null, "Cannot delete user with active orders", false);
                }

                if (user.Seller != null)
                {
                    var activeProducts = await _context.Products
    .Where(p => p.SellerId == user.Seller.SellerId && (p.IsAvailable == true))
    .AnyAsync();
                    if (activeProducts)
                        return new ApiResponse<string>(null, "Cannot delete user with active products", false);
                }

                // Handle Customer-related dependencies
                if (user.Customer != null)
                {
                    var customerId = user.Customer.CustomerId;
                    _context.Carts.RemoveRange(_context.Carts.Where(c => c.CustomerId == customerId));
                    _context.HelpfulRatings.RemoveRange(_context.HelpfulRatings.Where(hr => hr.CustomerId == customerId));
                    _context.Orders.RemoveRange(_context.Orders.Where(o => o.CustomerId == customerId));
                    _context.ProductViews.RemoveRange(_context.ProductViews.Where(pv => pv.CustomerId == customerId));
                    _context.Ratings.RemoveRange(_context.Ratings.Where(r => r.CustomerId == customerId));
                    _context.ReturnRequests.RemoveRange(_context.ReturnRequests.Where(rr => rr.CustomerId == customerId));
                    _context.SearchHistories.RemoveRange(_context.SearchHistories.Where(sh => sh.CustomerId == customerId));
                    _context.UserCoupons.RemoveRange(_context.UserCoupons.Where(uc => uc.CustomerId == customerId));
                    _context.UserProductInteractions.RemoveRange(_context.UserProductInteractions.Where(upi => upi.CustomerId == customerId));
                    _context.UserRecommendations.RemoveRange(_context.UserRecommendations.Where(ur => ur.CustomerId == customerId));
                    _context.Wishlists.RemoveRange(_context.Wishlists.Where(w => w.CustomerId == customerId));
                    _context.Customers.Remove(user.Customer);
                }

                // Handle Seller-related dependencies
                if (user.Seller != null)
                {
                    var sellerId = user.Seller.SellerId;
                    _context.AffiliateCommissions.RemoveRange(_context.AffiliateCommissions.Where(ac => ac.SellerId == sellerId));
                    _context.AffiliateSellerRelationships.RemoveRange(_context.AffiliateSellerRelationships.Where(asr => asr.SellerId == sellerId));
                    _context.Products.RemoveRange(_context.Products.Where(p => p.SellerId == sellerId));
                    _context.SubOrders.RemoveRange(_context.SubOrders.Where(so => so.SellerId == sellerId));
                    _context.Sellers.Remove(user.Seller);
                }

                // Handle Affiliate-related dependencies
                if (user.Affiliates.Any())
                {
                    foreach (var affiliate in user.Affiliates)
                    {
                        var affiliateId = affiliate.AffiliateId;
                        _context.AffiliateCommissions.RemoveRange(_context.AffiliateCommissions.Where(ac => ac.AffiliateId == affiliateId));
                        _context.AffiliateSellerRelationships.RemoveRange(_context.AffiliateSellerRelationships.Where(asr => asr.AffiliateId == affiliateId));
                        _context.AffiliateWithdrawals.RemoveRange(_context.AffiliateWithdrawals.Where(aw => aw.AffiliateId == affiliateId));
                        _context.Orders.RemoveRange(_context.Orders.Where(o => o.AffiliateId == affiliateId));
                    }
                    _context.Affiliates.RemoveRange(user.Affiliates);
                }

                // Remove direct dependencies
                if (user.Addresses.Any())
                    _context.Addresses.RemoveRange(user.Addresses);
                if (user.Admin != null)
                    _context.Admins.Remove(user.Admin);

                // Finally, remove the user
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return new ApiResponse<string>("User deleted", "User and all related data deleted successfully", true);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ApiResponse<string>(null, $"Error deleting user: {ex.Message}", false);
            }
        }

        public async Task<ApiResponse<string>> ChangePasswordAsync(int id, ChangePasswordDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return new ApiResponse<string>(null, "User not found", false);

            var verificationResult = _userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.CurrentPassword);
            if (verificationResult != PasswordVerificationResult.Success)
                return new ApiResponse<string>(null, "Current password is incorrect", false);

            user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, dto.NewPassword);
            await _context.SaveChangesAsync();
            return new ApiResponse<string>("Password changed", "Password changed successfully", true);
        }
    }
}