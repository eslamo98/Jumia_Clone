using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Models.DTOs.AdminDTOs;
using Jumia_Clone.Models.DTOs.RatingDTOs;   
using Jumia_Clone.Models.DTOs.CouponDTOs;
using Jumia_Clone.Models.DTOs.CustomerDTOs;
using Jumia_Clone.Models.DTOs.ReturnItemDTOs;
using Jumia_Clone.Models.DTOs.ReturnRequestDTOs;
using Jumia_Clone.Models.DTOs.SearchHistoryDTOs;
using Jumia_Clone.Models.DTOs.SellerDTOs;
using Jumia_Clone.Models.DTOs.UserDTOs;
using Jumia_Clone.Models.DTOs.UserCouponDTOs;
using Jumia_Clone.Models.DTOs.SearchResultClickDTOs;
using Jumia_Clone.Models.DTOs.TrendingProductDTOs;
using Jumia_Clone.Models.DTOs.UserProductInteractionDTOs;
using Jumia_Clone.Models.DTOs.UserRecommendationDTOs;
using Jumia_Clone.Models.DTOs.WishlistItemDTOs;
namespace Jumia_Clone.Repositories.Interfaces
{
    public interface IGetAllRepository
    {
        Task<IEnumerable<Admindto>> GetAllAdmins(PaginationDto pagination);
        Task<IEnumerable<CouponDto>> GetAllCoupons(PaginationDto pagination);
        Task<IEnumerable<Customerdto>> GetAllCustomers(PaginationDto pagination);
        Task<IEnumerable<Ratingdto>> GetAllRatings(PaginationDto pagination);
        Task<IEnumerable<ReturnItemdto>> GetAllReturnItems(PaginationDto pagination);
        Task<IEnumerable<ReturnRequestdto>> GetAllReturnRequests(PaginationDto pagination);
        Task<IEnumerable<SearchHistorydto>> GetAllSearchHistory(PaginationDto pagination);
        Task<IEnumerable<SearchResultClickdto>> GetAllSearchResultClicks(PaginationDto pagination);
        Task<IEnumerable<Sellerdto>> GetAllsellers(PaginationDto pagination);
        Task<IEnumerable<UserCoupondto>> GetAllUserCoupons(PaginationDto pagination);
        Task<IEnumerable<UserProductInteractiondto>> GetAllUserProductInteractions(PaginationDto pagination);
        Task<IEnumerable<UserRecommendationdto>> GetAllUserRecommendations(PaginationDto pagination);
        Task<IEnumerable<WishlistItemDto>> GetAllWishlistItems(PaginationDto pagination);
        Task<IEnumerable<TrendingProductdto>> GetTrendingProducts(PaginationDto pagination);
    }

}
