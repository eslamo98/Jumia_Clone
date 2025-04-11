using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Models.DTOs.AdminDTOs;
using Jumia_Clone.Models.DTOs.CartDTOs;
using Jumia_Clone.Models.DTOs.RatingDTOs;   
using Jumia_Clone.Models.DTOs.CouponDTOs;
using Jumia_Clone.Models.DTOs.CustomerDTOs;
using Jumia_Clone.Models.DTOs.ReturnItemDTOs;
using Jumia_Clone.Models.DTOs.ReturnRequestDTOs;
using Jumia_Clone.Models.DTOs.SearchHistoryDTOs;
using Jumia_Clone.Models.DTOs.SellerDTOs;
using Jumia_Clone.Models.DTOs.UserDTOs;
using Jumia_Clone.Models.DTOs.UserCouponDTOs;
using Jumia_Clone.Models.Entities;
using Jumia_Clone.Models.DTOs.SearchResultClickDTOs;
using Jumia_Clone.Models.DTOs.TrendingProductDTOs;
using Jumia_Clone.Models.DTOs.UserProductInteractionDTOs;
using Jumia_Clone.Models.DTOs.UserRecommendationDTOs;
using Jumia_Clone.Models.DTOs.WishlistItemDTOs;
namespace Jumia_Clone.Repositories.Interfaces
{
    public interface IGetAllRepository
    {
        Task<IEnumerable<Admindto>> GetAllAdmins(int AdminId,PaginationDto pagination);
        Task<IEnumerable<AdminProductReviewdto>> GetAllAdminProductReviews(int AdminProductReviewId, PaginationDto pagination);
        Task<IEnumerable<Coupondto>> GetAllCoupons(int CouponId, PaginationDto pagination);
        Task<IEnumerable<Customerdto>> GetAllCustomers(int CustomerId, PaginationDto pagination);
        Task<IEnumerable<Ratingdto>>GetAllRatings(int RatingId, PaginationDto pagination);
        Task<IEnumerable<ReturnItemdto>> GetAllReturnItems(int ReturnItemId, PaginationDto pagination);
        Task<IEnumerable<ReturnRequestdto>> GetAllReturnRequests(int ReturnRequestId, PaginationDto pagination);
        Task<IEnumerable<SearchHistorydto>> GetAllSearchHistory(int SearchId, PaginationDto pagination);
        Task<IEnumerable<SearchResultClickdto>> GetAllSearchResultClicks(int ClickId, PaginationDto pagination);
        Task<IEnumerable<Sellerdto>> GetAllsellers(int sellerId, PaginationDto pagination);
        Task<IEnumerable<TrendingProductdto>> GetTrendingProducts(int TrendingId, PaginationDto pagination);
        Task<IEnumerable<Userdto>> GetAllUsers(int UserId, PaginationDto pagination);
        Task<IEnumerable<UserCoupondto>> GetAllUserCoupons( int UserCouponId, PaginationDto pagination);
        Task<IEnumerable<UserProductInteractiondto>> GetAllUserProductInteractions(int InteractionId, PaginationDto pagination);
        Task<IEnumerable<UserRecommendationdto>> GetAllUserRecommendations(int UserRecommendationId, PaginationDto pagination);
        Task<IEnumerable<WishlistItemdto>> GetAllWishlistItems(int WishlistItemId, PaginationDto pagination);




    }
}
