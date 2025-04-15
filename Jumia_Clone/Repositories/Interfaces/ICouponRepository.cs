using Jumia_Clone.Models.DTOs.CouponDTOs;
using Jumia_Clone.Models.DTOs.GeneralDTOs;

namespace Jumia_Clone.Repositories.Interfaces
{
    public interface ICouponRepository
    {
        // Coupon CRUD operations
        Task<IEnumerable<CouponDto>> GetAllCouponsAsync(PaginationDto pagination, bool? isActive = null, string searchTerm = null);
        Task<int> GetCouponsCountAsync(bool? isActive = null, string searchTerm = null);
        Task<CouponDto> GetCouponByIdAsync(int couponId);
        Task<CouponDto> GetCouponByCodeAsync(string code);
        Task<CouponDto> CreateCouponAsync(CreateCouponDto couponDto);
        Task<CouponDto> UpdateCouponAsync(UpdateCouponDto couponDto);
        Task<bool> DeleteCouponAsync(int couponId);
        Task<bool> CouponExistsAsync(int couponId);
        Task<bool> CouponCodeExistsAsync(string code);

        // Coupon validation
        Task<CouponValidationResultDto> ValidateCouponAsync(ValidateCouponDto validateDto);

        // Coupon usage tracking
        Task<bool> IncrementCouponUsageAsync(int couponId);

        // User coupon operations
        Task<UserCouponDto> AssignCouponToUserAsync(AssignCouponDto assignDto);
        Task<IEnumerable<UserCouponDto>> GetUserCouponsAsync(int customerId, bool includeUsed = false);
        Task<bool> MarkCouponAsUsedAsync(int userCouponId);
        Task<bool> UserHasCouponAsync(int customerId, int couponId);
        Task<UserCouponDto> GetUserCouponByIdAsync(int userCouponId);
    }
}
