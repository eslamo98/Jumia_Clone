using Jumia_Clone.Data;
using Jumia_Clone.Models.DTOs.AdminDTOs;
using Jumia_Clone.Models.DTOs.CouponDTOs;
using Jumia_Clone.Models.DTOs.CustomerDTOs;
using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Models.DTOs.RatingDTOs;
using Jumia_Clone.Models.DTOs.ReturnItemDTOs;
using Jumia_Clone.Models.DTOs.ReturnRequestDTOs;
using Jumia_Clone.Models.DTOs.SearchHistoryDTOs;
using Jumia_Clone.Models.DTOs.SearchResultClickDTOs;
using Jumia_Clone.Models.DTOs.SellerDTOs;
using Jumia_Clone.Models.DTOs.TrendingProductDTOs;
using Jumia_Clone.Models.DTOs.UserCouponDTOs;
using Jumia_Clone.Models.DTOs.UserProductInteractionDTOs;
using Jumia_Clone.Models.DTOs.UserRecommendationDTOs;
using Jumia_Clone.Models.DTOs.WishlistItemDTOs;
using Jumia_Clone.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Jumia_Clone.Repositories.Implementation
{
    public class GetAllRepository : IGetAllRepository
    {
        private readonly ApplicationDbContext _context;
        public GetAllRepository(ApplicationDbContext context)
        {
            _context = context;
        }

      
        public async Task<IEnumerable<Admindto>> GetAllAdmins(PaginationDto pagination)
        {
            var query = _context.Admins.AsQueryable();

            if (pagination != null)
            {
                query = query
                    .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                    .Take(pagination.PageSize);
            }

            var admins = await query
                .Select(a => new Admindto
                {
                    AdminId = a.AdminId,
                    UserId = a.UserId,
                    Role = a.Role,
                    Permissions = a.Permissions
                })
                .ToListAsync();

            return admins ?? new List<Admindto>();
        }

        public async Task<IEnumerable<CouponDto>> GetAllCoupons(PaginationDto pagination)
        {
            var query = _context.Coupons.AsQueryable();

            if (pagination != null)
            {
                query = query
                    .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                    .Take(pagination.PageSize);
            }

            var coupons = await query
                .Select(c => new CouponDto
                {
                    CouponId = c.CouponId,
                    Code = c.Code,
                    Description = c.Description,
                    DiscountAmount = c.DiscountAmount,
                    MinimumPurchase = c.MinimumPurchase,
                    DiscountType = c.DiscountType,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    IsActive = c.IsActive,
                    UsageLimit = c.UsageLimit,
                    UsageCount = c.UsageCount
                })
                .ToListAsync();

            return coupons ?? new List<CouponDto>();
        }

        public async Task<IEnumerable<Customerdto>> GetAllCustomers(PaginationDto pagination)
        {
            var query = _context.Customers.AsQueryable();

            if (pagination != null)
            {
                query = query
                    .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                    .Take(pagination.PageSize);
            }

            var customers = await query
                .Select(c => new Customerdto
                {
                    CustomerId = c.CustomerId,
                    UserId = c.UserId,
                    LastLogin = c.LastLogin
                })
                .ToListAsync();

            return customers ?? new List<Customerdto>();
        }

        public async Task<IEnumerable<Ratingdto>> GetAllRatings(PaginationDto pagination)
        {
            var query = _context.Ratings.AsQueryable();

            if (pagination != null)
            {
                query = query
                    .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                    .Take(pagination.PageSize);
            }

            var ratings = await query
                .Select(r => new Ratingdto
                {
                    RatingId = r.RatingId,
                    CustomerId = r.CustomerId,
                    ProductId = r.ProductId,
                    Stars = r.Stars,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    IsVerifiedPurchase = r.IsVerifiedPurchase,
                    HelpfulCount = r.HelpfulCount
                })
                .ToListAsync();

            return ratings ?? new List<Ratingdto>();
        }

        public async Task<IEnumerable<ReturnItemdto>> GetAllReturnItems(PaginationDto pagination)
        {
            var query = _context.ReturnItems.AsQueryable();

            if (pagination != null)
            {
                query = query
                    .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                    .Take(pagination.PageSize);
            }

            var returnItems = await query
                .Select(ri => new ReturnItemdto
                {
                    ReturnItemId = ri.ReturnItemId,
                    ReturnId = ri.ReturnId,
                    OrderItemId = ri.OrderItemId,
                    Quantity = ri.Quantity,
                    ReturnReason = ri.ReturnReason,
                    Condition = ri.Condition,
                    RefundAmount = ri.RefundAmount
                })
                .ToListAsync();

            return returnItems ?? new List<ReturnItemdto>();
        }

        public async Task<IEnumerable<ReturnRequestdto>> GetAllReturnRequests(PaginationDto pagination)
        {
            var query = _context.ReturnRequests.AsQueryable();

            if (pagination != null)
            {
                query = query
                    .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                    .Take(pagination.PageSize);
            }

            var returnRequests = await query
                .Select(rr => new ReturnRequestdto
                {
                    ReturnId = rr.ReturnId,
                    SuborderId = rr.SuborderId,
                    CustomerId = rr.CustomerId,
                    ReturnReason = rr.ReturnReason,
                    ReturnStatus = rr.ReturnStatus,
                    RequestedAt = rr.RequestedAt,
                    ApprovedAt = rr.ApprovedAt,
                    ReceivedAt = rr.ReceivedAt,
                    RefundAmount = rr.RefundAmount,
                    RefundedAt = rr.RefundedAt,
                    TrackingNumber = rr.TrackingNumber,
                    Comments = rr.Comments
                })
                .ToListAsync();

            return returnRequests ?? new List<ReturnRequestdto>();
        }

        public async Task<IEnumerable<SearchHistorydto>> GetAllSearchHistory(PaginationDto pagination)
        {
            var query = _context.SearchHistories.AsQueryable();

            if (pagination != null)
            {
                query = query
                    .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                    .Take(pagination.PageSize);
            }

            var searchHistory = await query
                .Select(sh => new SearchHistorydto
                {
                    SearchId = sh.SearchId,
                    CustomerId = sh.CustomerId,
                    SessionId = sh.SessionId,
                    SearchQuery = sh.SearchQuery,
                    SearchTime = sh.SearchTime,
                    ResultCount = sh.ResultCount,
                    CategoryId = sh.CategoryId,
                    SubcategoryId = sh.SubcategoryId,
                    Filters = sh.Filters
                })
                .ToListAsync();

            return searchHistory ?? new List<SearchHistorydto>();
        }

        public async Task<IEnumerable<SearchResultClickdto>> GetAllSearchResultClicks(PaginationDto pagination)
        {
            var query = _context.SearchResultClicks.AsQueryable();

            if (pagination != null)
            {
                query = query
                    .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                    .Take(pagination.PageSize);
            }

            var searchResultClicks = await query
                .Select(src => new SearchResultClickdto
                {
                    ClickId = src.ClickId,
                    SearchId = src.SearchId,
                    ProductId = src.ProductId,
                    ClickTime = src.ClickTime,
                    PositionInResults = src.PositionInResults
                })
                .ToListAsync();

            return searchResultClicks ?? new List<SearchResultClickdto>();
        }

        public async Task<IEnumerable<Sellerdto>> GetAllsellers(PaginationDto pagination)
        {
            var query = _context.Sellers.AsQueryable();

            if (pagination != null)
            {
                query = query
                    .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                    .Take(pagination.PageSize);
            }

            var sellers = await query
                .Select(s => new Sellerdto
                {
                    SellerId = s.SellerId,
                    UserId = s.UserId,
                    BusinessName = s.BusinessName,
                    BusinessDescription = s.BusinessDescription,
                    BusinessLogo = s.BusinessLogo,
                    IsVerified = s.IsVerified,
                    VerifiedAt = s.VerifiedAt,
                    Rating = s.Rating
                })
                .ToListAsync();

            return sellers ?? new List<Sellerdto>();
        }

        public async Task<IEnumerable<UserCoupondto>> GetAllUserCoupons(PaginationDto pagination)
        {
            var query = _context.UserCoupons.AsQueryable();

            if (pagination != null)
            {
                query = query
                    .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                    .Take(pagination.PageSize);
            }

            var userCoupons = await query
                .Select(uc => new UserCoupondto
                {
                    UserCouponId = uc.UserCouponId,
                    CustomerId = uc.CustomerId,
                    CouponId = uc.CouponId,
                    IsUsed = uc.IsUsed,
                    AssignedAt = uc.AssignedAt,
                    UsedAt = uc.UsedAt
                })
                .ToListAsync();

            return userCoupons ?? new List<UserCoupondto>();
        }

        public async Task<IEnumerable<UserProductInteractiondto>> GetAllUserProductInteractions(PaginationDto pagination)
        {
            var query = _context.UserProductInteractions.AsQueryable();

            if (pagination != null)
            {
                query = query
                    .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                    .Take(pagination.PageSize);
            }

            var userProductInteractions = await query
                .Select(up => new UserProductInteractiondto
                {
                    InteractionId = up.InteractionId,
                    CustomerId = up.CustomerId,
                    SessionId = up.SessionId,
                    ProductId = up.ProductId,
                    InteractionType = up.InteractionType,
                    InteractionTime = up.InteractionTime,
                    DurationSeconds = up.DurationSeconds,
                    InteractionData = up.InteractionData
                })
                .ToListAsync();

            return userProductInteractions ?? new List<UserProductInteractiondto>();
        }

        public async Task<IEnumerable<UserRecommendationdto>> GetAllUserRecommendations(PaginationDto pagination)
        {
            var query = _context.UserRecommendations.AsQueryable();

            if (pagination != null)
            {
                query = query
                    .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                    .Take(pagination.PageSize);
            }

            var userRecommendations = await query
                .Select(ur => new UserRecommendationdto
                {
                    UserRecommendationId = ur.UserRecommendationId,
                    CustomerId = ur.CustomerId,
                    ProductId = ur.ProductId,
                    RecommendationType = ur.RecommendationType,
                    Score = ur.Score,
                    LastUpdated = ur.LastUpdated
                })
                .ToListAsync();

            return userRecommendations ?? new List<UserRecommendationdto>();
        }



        public async Task<IEnumerable<WishlistItemDto>> GetAllWishlistItems(PaginationDto pagination)
        {
            var query = _context.WishlistItems.AsQueryable();

            if (pagination != null)
            {
                query = query
                    .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                    .Take(pagination.PageSize);
            }

            var wishlistItems = await query
                .Select(wi => new WishlistItemDto
                {
                    WishlistItemId = wi.WishlistItemId,
                    WishlistId = wi.WishlistId,
                    ProductId = wi.ProductId,
                    AddedAt = wi.AddedAt
                })
                .ToListAsync();

            return wishlistItems ?? new List<WishlistItemDto>();
        }

        public async Task<IEnumerable<TrendingProductdto>> GetTrendingProducts(PaginationDto pagination)
        {
            var query = _context.TrendingProducts.AsQueryable();

            if (pagination != null)
            {
                query = query
                    .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                    .Take(pagination.PageSize);
            }

            var trendingProducts = await query
                .Select(tp => new TrendingProductdto
                {
                    TrendingId = tp.TrendingId,
                    ProductId = tp.ProductId,
                    CategoryId = tp.CategoryId,
                    SubcategoryId = tp.SubcategoryId,
                    TrendScore = tp.TrendScore,
                    TimePeriod = tp.TimePeriod,
                    LastUpdated = tp.LastUpdated
                })
                .ToListAsync();

            return trendingProducts ?? new List<TrendingProductdto>();
        }
    }
}