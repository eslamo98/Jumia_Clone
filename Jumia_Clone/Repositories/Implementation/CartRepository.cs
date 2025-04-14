using Jumia_Clone.Data;
using Jumia_Clone.Models.DTOs.CartDTOs;
using Jumia_Clone.Models.DTOs.CartItemDtos;
using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Models.Entities;
using Jumia_Clone.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jumia_Clone.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _context;

        public CartRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CartDto> GetOrCreateCartAsync(int customerId, PaginationDto pagination)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.Seller)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Variant)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (cart == null)
            {
                cart = new Cart
                {
                    CustomerId = customerId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var cartItems = cart.CartItems.AsQueryable();

            if (pagination != null)
            {
                cartItems = cartItems
                   .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                    .Take(pagination.PageSize);
            }

            var cartDto = new CartDto
            {
                CartId = cart.CartId,
                Items = await cartItems.Select(ci => new CartItemDto
                {
                    CartItemId = ci.CartItemId,
                    ProductId = ci.ProductId,
                    VariantId = ci.VariantId ?? 0,
                    Quantity = ci.Quantity,
                    Price = ci.PriceAtAddition,
                    Total = ci.Quantity * ci.PriceAtAddition
                }).ToListAsync(),
                Summary = new CartSummaryDto
                {
                    Subtotal = cart.CartItems.Sum(ci => ci.Quantity * ci.PriceAtAddition),
                    TotalItems = cart.CartItems.Sum(ci => ci.Quantity),
                    SellerCount = cart.CartItems
                        .Select(ci => ci.Product.SellerId)
                        .Distinct()
                        .Count()
                }
            };

            return cartDto;
        }

        public async Task<CartItemDto> AddItemToCartAsync(int customerId, AddItemToCartDto addItemDto)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId)
                ?? await CreateCartForCustomerAsync(customerId);

            var existingItem = cart.CartItems.FirstOrDefault(ci =>
                ci.ProductId == addItemDto.ProductId &&
                ci.VariantId == addItemDto.VariantId);

            if (existingItem != null)
            {
                existingItem.Quantity += addItemDto.Quantity;
                existingItem.PriceAtAddition = addItemDto.PriceAtAddition;
            }
            else
            {
                existingItem = new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = addItemDto.ProductId,
                    VariantId = addItemDto.VariantId,
                    Quantity = existingItem.Quantity,
                    PriceAtAddition = addItemDto.PriceAtAddition
                };
                _context.CartItems.Add(existingItem);
            }

            await _context.SaveChangesAsync();

            return new CartItemDto
            {
                CartItemId = existingItem.CartItemId,
                ProductId = existingItem.ProductId,
                VariantId = existingItem.VariantId ?? 0,
                Quantity = existingItem.Quantity,
                Price = existingItem.PriceAtAddition,
                Total = existingItem.Quantity * existingItem.PriceAtAddition
            };
        }


        public async Task<CartItemDto> UpdateCartItemAsync(UpdateCartItemDto updateItemDto)
        {
            var cartItem = await _context.CartItems.FindAsync(updateItemDto.CartItemId);
            if (cartItem == null) return null;

            cartItem.Quantity = updateItemDto.Quantity;
            _context.CartItems.Update(cartItem);
            await _context.SaveChangesAsync();

            return new CartItemDto
            {
                CartItemId = cartItem.CartItemId,
                ProductId = cartItem.ProductId,
                VariantId = cartItem.VariantId ?? 0,
                Quantity = cartItem.Quantity,
                Price = cartItem.PriceAtAddition,
                Total = cartItem.Quantity * cartItem.PriceAtAddition
            };
        }

        public async Task<bool> RemoveItemFromCartAsync(int cartItemId)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var cartItem = await _context.CartItems.FindAsync(cartItemId);
                    if (cartItem == null) return false;

                    _context.CartItems.Remove(cartItem);
                    await _context.SaveChangesAsync();

                    // Update cart summary
                    var cart = await _context.Carts
                        .Include(c => c.CartItems)
                        .FirstOrDefaultAsync(c => c.CartId == cartItem.CartId);

                    if (cart != null)
                    {
                        cart.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();
                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<bool> ClearCartAsync(int customerId)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var cart = await _context.Carts
                        .Include(c => c.CartItems)
                        .FirstOrDefaultAsync(c => c.CustomerId == customerId);

                    if (cart == null) return false;

                    _context.CartItems.RemoveRange(cart.CartItems);
                    cart.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<CartDto> GetCartAsync(int customerId, PaginationDto pagination)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .ThenInclude(p => p.ProductVariants)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);
            if (cart == null) return null;
            pagination = pagination ?? new PaginationDto();
            var cartItems = await _context.CartItems
                .Where(ci => ci.CartId == cart.CartId)
                .Skip(pagination.PageSize * pagination.PageNumber)
                .Take(pagination.PageSize)
                .Select(ci => new CartItemDto
                {
                    CartItemId = ci.CartItemId,
                    ProductId = ci.ProductId,
                    VariantId = ci.VariantId ?? 0,
                    Quantity = ci.Quantity,
                    Price = ci.PriceAtAddition,
                    Total = ci.Quantity * ci.PriceAtAddition
                })
                .ToListAsync();
            return new CartDto
            {
                CartId = cart.CartId,
                CustomerId = cart.CustomerId,
                Items = cartItems
            };
        }
        public async Task<CartItemDto> GetCartItemAsync(int cartItemId)
        {
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);

            if (cartItem == null) return null;

            return new CartItemDto
            {
                CartItemId = cartItem.CartItemId,
                ProductId = cartItem.ProductId,
                VariantId = cartItem.VariantId ?? 0,
                Quantity = cartItem.Quantity,
                Price = cartItem.PriceAtAddition,
                Total = cartItem.Quantity * cartItem.PriceAtAddition
            };
        }

        private async Task<Cart> CreateCartForCustomerAsync(int customerId)
        {
            var cart = new Cart
            {
                CustomerId = customerId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
            return cart;
        }
        public async Task<CartDto> GetCartProducts(List<CartItem> cartItems, PaginationDto pagination)
        {
            var productIds = cartItems.Select(ci => ci.ProductId).ToList();
            var variantIds = cartItems.Select(ci => ci.VariantId).ToList();

       
            var products = await _context.Products
                .Where(p => productIds.Contains(p.ProductId))
                .Where(p => p.ProductVariants.Any(v => variantIds.Contains(v.VariantId)))
                .Include(p => p.ProductVariants)
                .ToListAsync();

            var pagedItems = cartItems
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToList();
            var cartDto = new CartDto
            {
                Items = pagedItems.Select(ci =>
                {
                    // Find the corresponding product and variant
                    var product = products.FirstOrDefault(p => p.ProductId == ci.ProductId);
                    var variant = product?.ProductVariants.FirstOrDefault(v => v.VariantId == ci.VariantId);

                    return new CartItemDto
                    {
                        CartItemId = ci.CartItemId,
                        ProductId = ci.ProductId,
                        VariantId = ci.VariantId ?? 0,
                        Quantity = ci.Quantity,
                        Price = ci.PriceAtAddition,
                        Total = ci.Quantity * ci.PriceAtAddition,
                        
                    };
                }).ToList()
            };

            return cartDto;
        }



        

        public Task<bool> AddToCart(CartItem cartItem)
        {
            throw new NotImplementedException();
        }
    }
}