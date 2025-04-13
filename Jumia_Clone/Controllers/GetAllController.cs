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
using Jumia_Clone.Models.DTOs.UserProductInteractionDTOs;
using Jumia_Clone.Models.DTOs.UserRecommendationDTOs;
using Jumia_Clone.Models.DTOs.WishlistItemDTOs;
using Jumia_Clone.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Jumia_Clone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GetAllController : ControllerBase
    {
        private readonly IGetAllRepository _getAllRepository;
        public GetAllController(IGetAllRepository getAllRepository)
        {
            _getAllRepository = getAllRepository;
        }


        [HttpGet("admins")]
        public async Task<IActionResult> GetAllAdmins([FromQuery] PaginationDto pagination)
        {
            try
            { 
                var admins = await _getAllRepository.GetAllAdmins(pagination);

                return Ok(new ApiResponse<IEnumerable<Admindto>>
                {
                    Message = "Successfully retrieved all admins.",
                    Data = admins,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse()
                {
                    Message = "An error occurred while retrieving admins.",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        [HttpGet("coupons")]
        public async Task<IActionResult> GetAllCoupons([FromQuery] PaginationDto pagination)
        {
            try
            {
                var coupons = await _getAllRepository.GetAllCoupons(pagination);

                return Ok(new ApiResponse<IEnumerable<Coupondto>>
                {
                    Message = "Successfully retrieved all coupons.",
                    Data = coupons,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse()
                {
                    Message = "An error occurred while retrieving coupons.",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

       
        [HttpGet("Customers")]
        public async Task<IActionResult> GetAllCustomers([FromQuery] PaginationDto pagination)
        {
            try
            {
                var customers = await _getAllRepository.GetAllCustomers(pagination);

                return Ok(new ApiResponse<IEnumerable<Customerdto>>
                {
                    Message = "Successfully retrieved all customers.",
                    Data = customers,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse()
                {
                    Message = "An error occurred while retrieving customers.",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        
        [HttpGet("Ratings")]
        public async Task<IActionResult> GetAllRatings([FromQuery] PaginationDto pagination)
        {
            try
            {
                var ratings = await _getAllRepository.GetAllRatings(pagination);

                return Ok(new ApiResponse<IEnumerable<Ratingdto>>
                {
                    Message = "Successfully retrieved all ratings.",
                    Data = ratings,
                    Success = true
                });
            }
            catch (Exception ex)
            {
 
                return StatusCode(500, new ApiErrorResponse()
                {
                    Message = "An error occurred while retrieving ratings.",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }
       
        [HttpGet("ReturnItems")]
        public async Task<IActionResult> GetAllReturnItems([FromQuery] PaginationDto pagination)
        {
            try
            { 
                var returnItems = await _getAllRepository.GetAllReturnItems(pagination);

                return Ok(new ApiResponse<IEnumerable<ReturnItemdto>>
                {
                    Message = "Successfully retrieved all return items.",
                    Data = returnItems,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse()
                {
                    Message = "An error occurred while retrieving return items.",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

    
        [HttpGet("ReturnRequests")]
        public async Task<IActionResult> GetAllReturnRequests([FromQuery] PaginationDto pagination)
        {
            try
            {
                var returnRequests = await _getAllRepository.GetAllReturnRequests(pagination);

                return Ok(new ApiResponse<IEnumerable<ReturnRequestdto>>
                {
                    Message = "Successfully retrieved all return requests.",
                    Data = returnRequests,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse()
                {
                    Message = "An error occurred while retrieving return requests.",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        [HttpGet("SearchHistory")]
        public async Task<IActionResult> GetAllSearchHistory([FromQuery] PaginationDto pagination)
        {
            try
            {
                var searchHistory = await _getAllRepository.GetAllSearchHistory(pagination);

                return Ok(new ApiResponse<IEnumerable<SearchHistorydto>>
                {
                    Message = "Successfully retrieved all search history.",
                    Data = searchHistory,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving search history.",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        [HttpGet("SearchResultClicks")]
        public async Task<IActionResult> GetAllSearchResultClicks([FromQuery] PaginationDto pagination)
        {
            try
            {
                var searchResultClicks = await _getAllRepository.GetAllSearchResultClicks(pagination);

                return Ok(new ApiResponse<IEnumerable<SearchResultClickdto>>
                {
                    Message = "Successfully retrieved all search result clicks.",
                    Data = searchResultClicks,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving search result clicks.",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        [HttpGet("Sellers")]
        public async Task<IActionResult> GetAllSellers([FromQuery] PaginationDto pagination)
        {
            try
            {
                var sellers = await _getAllRepository.GetAllsellers(pagination);

                return Ok(new ApiResponse<IEnumerable<Sellerdto>>
                {
                    Message = "Successfully retrieved all sellers.",
                    Data = sellers,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving sellers.",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        [HttpGet("UserCoupons")]
        public async Task<IActionResult> GetAllUserCoupons([FromQuery] PaginationDto pagination)
        {
            try
            {
                var userCoupons = await _getAllRepository.GetAllUserCoupons(pagination);

                return Ok(new ApiResponse<IEnumerable<UserCoupondto>>
                {
                    Message = "Successfully retrieved all user coupons.",
                    Data = userCoupons,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving user coupons.",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }


        [HttpGet("UserProductInteractions")]
        public async Task<IActionResult> GetAllUserProductInteractions([FromQuery] PaginationDto pagination)
        {
            try
            {
                var userProductInteractions = await _getAllRepository.GetAllUserProductInteractions(pagination);

                return Ok(new ApiResponse<IEnumerable<UserProductInteractiondto>>
                {
                    Message = "Successfully retrieved all user product interactions.",
                    Data = userProductInteractions,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving user product interactions.",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        [HttpGet("UserRecommendations")]
        public async Task<IActionResult> GetAllUserRecommendations([FromQuery] PaginationDto pagination)
        {
            try
            {
                var userRecommendations = await _getAllRepository.GetAllUserRecommendations(pagination);

                return Ok(new ApiResponse<IEnumerable<UserRecommendationdto>>
                {
                    Message = "Successfully retrieved all user recommendations.",
                    Data = userRecommendations,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving user recommendations.",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }
        [HttpGet("Users")]
        public async Task<IActionResult> GetAllUsers([FromQuery] PaginationDto pagination)
        {
            try
            {
                var users = await _getAllRepository.GetAllUsers(pagination);

                return Ok(new ApiResponse<IEnumerable<Userdto>>
                {
                    Message = "Successfully retrieved all users.",
                    Data = users,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving users.",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }
        [HttpGet("WishlistItems")]
        public async Task<IActionResult> GetAllWishlistItems([FromQuery] PaginationDto pagination)
        {
            try
            {
                var wishlistItems = await _getAllRepository.GetAllWishlistItems(pagination);

                return Ok(new ApiResponse<IEnumerable<WishlistItemdto>>
                {
                    Message = "Successfully retrieved all wishlist items.",
                    Data = wishlistItems,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving wishlist items.",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }
        [HttpGet("TrendingProducts")]
        public async Task<IActionResult> GetTrendingProducts([FromQuery] PaginationDto pagination)
        {
            try
            {
                var trendingProducts = await _getAllRepository.GetTrendingProducts( pagination);

                return Ok(new ApiResponse<IEnumerable<TrendingProductdto>>
                {
                    Message = "Successfully retrieved all trending products.",
                    Data = trendingProducts,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving trending products.",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

    }
}
