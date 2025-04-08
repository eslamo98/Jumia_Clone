using Jumia_Clone.Models.Entities;

namespace Jumia_Clone.Repositories.Interfaces
{
    public interface ICartRepository
    {
        Task<Cart> GetCartByCustomerId(int customerId);
        Task<Cart> CreateCartForCustomer(int customerId);

        Task<CartItem> GetCartItem(int cartItemId);
        Task<CartItem> GetCartItem(int cartId, int productId, int? variantId);

        Task AddCartItem(CartItem item);
        Task UpdateCartItem(CartItem item);
        Task RemoveCartItem(CartItem item);
        Task ClearCart(int cartId);

        Task SaveChanges();
    }

}
