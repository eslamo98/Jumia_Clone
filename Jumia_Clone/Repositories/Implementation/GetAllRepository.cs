using Jumia_Clone.Data;
using Jumia_Clone.Models.DTOs;
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
using Jumia_Clone.Models.DTOs.UserDTOs;
using Jumia_Clone.Models.Entities;
using Jumia_Clone.Models.DTOs.UserProductInteractionDTOs;
using Jumia_Clone.Models.DTOs.UserRecommendationDTOs;
using Jumia_Clone.Models.DTOs.WishlistItemDTOs;
using Jumia_Clone.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Jumia_Clone.Models.DTOs.SubcategoryDTOs;

namespace Jumia_Clone.Repositories.Implementation
{
    public class GetAllRepository : IGetAllRepository
    {
        private readonly ApplicationDbContext _context;
        public GetAllRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AdminProductReviewdto>> GetAllAdminProductReviews(int AdminProductReviewId, PaginationDto pagination)
        {
            //var adminProductReviews = await _context.AdminProductReview
            //   .Where(r => r.ReviewId == ReviewId)
            //   .Select(r => new AdminProductReviewdto
            //   {
            //       ReviewId = r.ReviewId,
            //       ProductId = r.ProductId,
            //       AdminId = r.AdminId,
            //       PreviousStatus = r.PreviousStatus,
            //       NewStatus = r.NewStatus,
            //       Notes = r.Notes,
            //       ReviewedAt = r.ReviewedAt
            //   })
            //   .ToListAsync();
            //return adminProductReviews ?? new List<AdminProductReviewdto>();

            throw new NotImplementedException();
        }
        
        

        public async Task<IEnumerable<Admindto>> GetAllAdmins(int AdminId, PaginationDto pagination)
        {
            var admins = await _context.Admins
                .Where(a => a.AdminId == AdminId)
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

        public async Task<IEnumerable<Coupondto>> GetAllCoupons(int CouponId, PaginationDto pagination)
        {
            var coupons = await _context.Coupons
                .Where(c => c.CouponId == CouponId)
                .Select(c => new Coupondto
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
            return coupons ?? new List<Coupondto>();
        }

        public  async Task<IEnumerable<Customerdto>> GetAllCustomers(int CustomerId, PaginationDto pagination)
        {
            var customers = await _context.Customers
                .Where(c => c.CustomerId == CustomerId)
                .Select(c => new Customerdto
                {
                    CustomerId = c.CustomerId,
                    UserId = c.UserId,
                    LastLogin = c.LastLogin
                })
                .ToListAsync();
            return customers ?? new List<Customerdto>();

        }

        public async  Task<IEnumerable<Ratingdto>> GetAllRatings(int RatingId, PaginationDto pagination)
        {
            var ratings = await _context.Ratings
                .Where(r => r.RatingId == RatingId)
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

        public  async Task<IEnumerable<ReturnItemdto>> GetAllReturnItems(int ReturnItemId, PaginationDto pagination)
        {
            var returnItems = await _context.ReturnItems
                .Where(ri => ri.ReturnItemId == ReturnItemId)
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

        public async  Task<IEnumerable<ReturnRequestdto>> GetAllReturnRequests(int ReturnRequestId, PaginationDto pagination)
        {
            var returnRequests = await _context.ReturnRequests
                .Where(rr => rr.ReturnId == ReturnRequestId)
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

        public async Task<IEnumerable<SearchHistorydto>> GetAllSearchHistory(int SearchId, PaginationDto pagination)
        {
            var searchHistory = await _context.SearchHistories
                .Where(sh => sh.SearchId == SearchId)
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

        public async  Task<IEnumerable<SearchResultClickdto>> GetAllSearchResultClicks(int ClickId, PaginationDto pagination)
        {
            var searchResultClicks = await _context.SearchResultClicks
                .Where(src => src.ClickId == ClickId)
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

        public async Task<IEnumerable<Sellerdto>> GetAllsellers(int sellerId, PaginationDto pagination)
        {
          var sellers = await _context.Sellers
                .Where(s => s.SellerId == sellerId)
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

        public async Task<IEnumerable<UserCoupondto>> GetAllUserCoupons(int UserCouponId, PaginationDto pagination)
        {
            var userCoupons = await _context.UserCoupons
                .Where(uc => uc.UserCouponId == UserCouponId)
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

        public async Task<IEnumerable<UserProductInteractiondto>> GetAllUserProductInteractions(int InteractionId, PaginationDto pagination)
        {
           var userProductInteractions = await _context.UserProductInteractions
                .Where(up => up.InteractionId == InteractionId)
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

        public async  Task<IEnumerable<UserRecommendationdto>> GetAllUserRecommendations(int UserRecommendationId, PaginationDto pagination)
        {
           var userRecommendations = await _context.UserRecommendations
                .Where(ur => ur.UserRecommendationId == UserRecommendationId)
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

        public async  Task<IEnumerable<Userdto>> GetAllUsers(int UserId, PaginationDto pagination)
        {
            var users = await _context.Users
                .Where(u => u.UserId == UserId)
                .Select(u => new Userdto
                {
                    UserId = u.UserId,
                    Email = u.Email,
                    PasswordHash = u.PasswordHash,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    PhoneNumber = u.PhoneNumber,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    UserType = u.UserType,
                    IsActive = u.IsActive
                })
                .ToListAsync();
            return users ?? new List<Userdto>();
        }

        public async  Task<IEnumerable<WishlistItemdto>> GetAllWishlistItems(int WishlistItemId, PaginationDto pagination)
        {
            var wishlistItems = await _context.WishlistItems
                .Where(wi => wi.WishlistItemId == WishlistItemId)
                .Select(wi => new WishlistItemdto
                {
                    WishlistItemId = wi.WishlistItemId,
                    WishlistId = wi.WishlistId,
                    ProductId = wi.ProductId,
                    AddedAt = wi.AddedAt
                })
                .ToListAsync();
            return wishlistItems ?? new List<WishlistItemdto>();
        }

        public async  Task<IEnumerable<TrendingProductdto>> GetTrendingProducts(int TrendingId, PaginationDto pagination)
        {
            var trendingProducts = await _context.TrendingProducts
                .Where(tp => tp.TrendingId == TrendingId)
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
