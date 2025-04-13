using Jumia_Clone.Models.DTOs.CartDTOs;
using Jumia_Clone.Models.DTOs.CartItemDtos;
using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Repositories.Interfaces;
using Jumia_Clone.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Jumia_Clone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IImageService _imageService;

        public CartController(
            ICartRepository cartRepository,
            IProductRepository productRepository,
            IImageService imageService)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _imageService = imageService;
        }

        private int GetCurrentCustomerId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        // GET: api/cart
        [HttpGet]
        public async Task<IActionResult> GetCart([FromQuery] PaginationDto pagination)
        {
            try
            {
                var customerId = GetCurrentCustomerId();
                var cartDto = await _cartRepository.GetOrCreateCartAsync(customerId, pagination);

                // Enrich with product and variant details
                foreach (var item in cartDto.Items)
                {
                    var product = await _productRepository.GetProductByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        if (!string.IsNullOrEmpty(product.MainImageUrl))
                        {
                            product.MainImageUrl = _imageService.GetImageUrl(product.MainImageUrl);
                        }
                    }
                }

                return Ok(new ApiResponse<CartDto>
                {
                    Success = true,
                    Data = cartDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving the cart",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // POST: api/cart/items
        [HttpPost("items")]
        public async Task<IActionResult> AddItemToCart([FromBody] AddItemToCartDto addItemDto)
        {
            try
            {
                var customerId = GetCurrentCustomerId();
                var cartItemDto = await _cartRepository.AddItemToCartAsync(customerId, addItemDto);

                var cart = await _cartRepository.GetOrCreateCartAsync(customerId, null);
                var summary = new CartSummaryDto
                {
                    Subtotal = cart.Items.Sum(i => i.Total),
                    TotalItems = cart.Items.Sum(i => i.Quantity),
                    SellerCount = cart.Items
                        .Select(i => i.ProductId) // Would need to get seller from product
                        .Distinct()
                        .Count()
                };

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Item added to cart",
                    Data = new
                    {
                        cart_item_id = cartItemDto.CartItemId,
                        product_id = cartItemDto.ProductId,
                        variant_id = cartItemDto.VariantId,
                        quantity = cartItemDto.Quantity,
                        price = cartItemDto.Price,
                        cart_summary = summary
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while adding item to cart",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // PUT: api/cart/items/{id}
        [HttpPut("items/{id}")]
        public async Task<IActionResult> UpdateCartItem(int id, [FromBody] UpdateCartItemDto updateItemDto)
        {
            try
            {
                if (id != updateItemDto.CartItemId)
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        Message = "Cart item ID mismatch",
                        ErrorMessages = new string[] { "The ID in the URL does not match the ID in the request body" }
                    });
                }

                var customerId = GetCurrentCustomerId();
                var cartItemDto = await _cartRepository.UpdateCartItemAsync(updateItemDto);

                if (cartItemDto == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Cart item not found",
                        Success = false,
                        Data = null
                    });
                }

                var cart = await _cartRepository.GetOrCreateCartAsync(customerId, null);
                var summary = new CartSummaryDto
                {
                    Subtotal = cart.Items.Sum(i => i.Total),
                    TotalItems = cart.Items.Sum(i => i.Quantity),
                    SellerCount = cart.Items
                        .Select(i => i.ProductId) // Would need to get seller from product
                        .Distinct()
                        .Count()
                };

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Cart item updated",
                    Data = new
                    {
                        cart_item_id = cartItemDto.CartItemId,
                        quantity = cartItemDto.Quantity,
                        total = cartItemDto.Total,
                        cart_summary = summary
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while updating cart item",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }


        // DELETE: api/cart/items/{id}
        [HttpDelete("items/{id}")]
        public async Task<IActionResult> RemoveCartItem(int id)
        {
            try
            {
                var success = await _cartRepository.RemoveItemFromCartAsync(id);

                if (!success)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Cart item not found",
                        Success = false,
                        Data = null
                    });
                }

                var customerId = GetCurrentCustomerId();
                var cart = await _cartRepository.GetOrCreateCartAsync(customerId, null);
                var summary = CalculateCartSummary(cart);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Item removed from cart",
                    Data = new { cart_summary = summary }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while removing cart item",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // DELETE: api/cart
        [HttpDelete("clearcart")]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                var customerId = GetCurrentCustomerId();
                var success = await _cartRepository.ClearCartAsync(customerId);

                if (!success)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Cart not found",
                        Success = false,
                        Data = null
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Cart cleared successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while clearing the cart",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }
        private CartSummaryDto CalculateCartSummary(CartDto cart)
        {
            if (cart == null) return new CartSummaryDto();

            return new CartSummaryDto
            {
                Subtotal = cart.Items.Sum(i => i.Total),
                TotalItems = cart.Items.Sum(i => i.Quantity),
                SellerCount = cart.Items
                    .GroupBy(i => i.ProductId) // Group by product as proxy for seller
                    .Count()
            };
        }
    }
}