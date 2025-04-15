using Jumia_Clone.Models.DTOs.CartDTOs;
using Jumia_Clone.Models.DTOs.WishlistItemDTOs;

namespace Jumia_Clone.Repositories.Interfaces
{
    public interface IWishlistRepository
    {
        // Get wishlist for a customer
        Task<WishlistDto> GetWishlistByCustomerIdAsync(int customerId, bool includeItems = true);

        // Get wishlist summary (count of items)
        Task<WishlistSummaryDto> GetWishlistSummaryByCustomerIdAsync(int customerId);

        // Get a specific wishlist item
        Task<WishlistItemDto> GetWishlistItemByIdAsync(int wishlistItemId);

        // Check if an item exists in the wishlist
        Task<bool> WishlistItemExistsAsync(int customerId, int productId);

        // Check if a wishlist item belongs to customer
        Task<bool> WishlistItemBelongsToCustomerAsync(int customerId, int wishlistItemId);

        // Add item to wishlist
        Task<WishlistItemDto> AddItemToWishlistAsync(int customerId, AddWishlistItemDto itemDto);

        // Remove item from wishlist
        Task<bool> RemoveItemFromWishlistAsync(int wishlistItemId);

        // Clear all items from wishlist
        Task<bool> ClearWishlistAsync(int customerId);

        // Move item to cart
        Task<CartItemDto> MoveItemToCartAsync(int customerId, int wishlistItemId);

        // Move multiple items to cart
        Task<List<CartItemDto>> MoveMultipleItemsToCartAsync(int customerId, List<int> wishlistItemIds);

        // Get all out of stock wishlist items
        Task<List<WishlistItemDto>> GetOutOfStockWishlistItemsAsync(int customerId);

        // Get recently added wishlist items
        Task<List<WishlistItemDto>> GetRecentlyAddedWishlistItemsAsync(int customerId, int count);
    }
}
