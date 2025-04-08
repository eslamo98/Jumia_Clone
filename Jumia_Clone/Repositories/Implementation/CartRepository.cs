using Jumia_Clone.Data;
using Jumia_Clone.Models.Entities;
using Jumia_Clone.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Jumia_Clone.Repositories.Implementation
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _context;

        public CartRepository(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<Cart> GetCartByCustomerId(int customerId)
        {
            return await _context.Carts .Include(c => c.CartItems).ThenInclude(i => i.Product).Include(c => c.CartItems) .ThenInclude(i => i.Variant).FirstOrDefaultAsync(c => c.CustomerId == customerId);
        }

        public async Task<Cart> CreateCartForCustomer(int customerId)
        {
            var cart = new Cart
            {
                CustomerId = customerId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
            return cart;
        }

        public async Task<CartItem> GetCartItem(int cartItemId)
        {
            return await _context.CartItems.Include(i => i.Product)
                .Include(i => i.Variant)
                .FirstOrDefaultAsync(i => i.CartItemId == cartItemId);
        }

        public async Task<CartItem> GetCartItem(int cartId, int productId, int? variantId)
        {
            return await _context.CartItems.FirstOrDefaultAsync(i => i.CartId == cartId && i.ProductId == productId && i.VariantId == variantId);
        }

        public async Task AddCartItem(CartItem item)
        {
            await _context.CartItems.AddAsync(item);
        }

        public async Task UpdateCartItem(CartItem item)
        {
            _context.CartItems.Update(item);
        }

        public async Task RemoveCartItem(CartItem item)
        {
            _context.CartItems.Remove(item);
        }

        public async Task ClearCart(int cartId)
        {
            var items = await _context.CartItems.Where(i => i.CartId == cartId).ToListAsync();
            _context.CartItems.RemoveRange(items);
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }
    }


}
