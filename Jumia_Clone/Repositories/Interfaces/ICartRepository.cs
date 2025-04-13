using Jumia_Clone.Models.DTOs.CartDTOs;
using Jumia_Clone.Models.DTOs.CartItemDtos;
using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Models.Entities;
using System.Threading.Tasks;

namespace Jumia_Clone.Repositories.Interfaces
{
    public interface ICartRepository
    {
        // Get cart by customer ID with the option to include cart items
        Task<CartDto> GetCartByCustomerIdAsync(int userId, bool includeItems = true);

        // Get cart summary (total items, subtotal)
        Task<CartSummaryDto> GetCartSummaryByCustomerIdAsync(int customerId);

        // Get cart item by ID
        Task<CartItemDto> GetCartItemByIdAsync(int cartItemId);

        // Add item to cart
        Task<CartItemDto> AddCartItemAsync(int customerId, AddCartItemDto cartItemDto);

        // Update cart item quantity
        Task<CartItemDto> UpdateCartItemQuantityAsync(int customerId, UpdateCartItemDto updateCartItemDto);

        // Remove item from cart
        Task<bool> RemoveCartItemAsync(int customerId, int cartItemId);

        // Clear all items from cart
        Task<bool> ClearCartAsync(int customerId);

        // Check if product exists in cart
        Task<bool> ProductExistsInCartAsync(int customerId, int productId, int? variantId = null);

        // Check if cart item exists and belongs to the customer
        Task<bool> CartItemExistsAndBelongsToCustomerAsync(int customerId, int cartItemId);

        // Get cart items count
        Task<int> GetCartItemsCountAsync(int customerId);
    }
}