using AutoMapper;
using Jumia_Clone.CustomException;
using Jumia_Clone.Data;
using Jumia_Clone.Models.Constants;
using Jumia_Clone.Models.DTOs.CartDTOs;
using Jumia_Clone.Models.DTOs.CartItemDtos;
using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Models.DTOs.WishlistItemDTOs;
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
        private readonly IMapper _mapper;
        private readonly ILogger<CartRepository> _logger;

        public CartRepository(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<CartRepository> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CartDto> GetCartByCustomerIdAsync(int userId, bool includeItems = true)
        {
            try
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
                if (customer == null)
                    throw new KeyNotFoundException($"Customer with user ID {userId} not found or is not available");

                // Get or create the cart for this customer
                var cart = await GetOrCreateCartAsync(customer.CustomerId);

                // If we need to include items
                if (includeItems)
                {
                    // Load cart items with product information
                    await _context.Entry(cart)
                        .Collection(c => c.CartItems)
                        .LoadAsync();

                    // Load product information for each cart item
                    foreach (var item in cart.CartItems)
                    {
                        await _context.Entry(item)
                            .Reference(i => i.Product)
                            .LoadAsync();

                        // Load variant information if present
                        if (item.VariantId.HasValue)
                        {
                            await _context.Entry(item)
                                .Reference(i => i.Variant)
                                .LoadAsync();
                        }
                    }
                }

                // Map to DTO
                var cartDto = _mapper.Map<CartDto>(cart);

                // Add product name and image
                if (includeItems && cart.CartItems != null)
                {
                    foreach (var item in cart.CartItems)
                    {
                        var cartItemDto = cartDto.CartItems.FirstOrDefault(ci => ci.CartItemId == item.CartItemId);
                        if (cartItemDto != null)
                        {
                            cartItemDto.ProductName = item.Product.Name;
                            cartItemDto.ProductImage = item.Product.MainImageUrl;

                            if (item.VariantId.HasValue && item.Variant != null)
                            {
                                cartItemDto.VariantName = item.Variant.VariantName;
                                cartItemDto.ProductImage = item.Variant.VariantImageUrl;
                            }
                        }
                    }
                }

                return cartDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching cart for customer {CustomerId}", userId);
                throw;
            }
        }

        public async Task<CartSummaryDto> GetCartSummaryByCustomerIdAsync(int userId)
        {
            try
            {
                var customerId = getCutomerId(userId);
                if (customerId == 0)
                {
                    throw new KeyNotFoundException("customer was not found"); }
                var cart = await GetOrCreateCartAsync(customerId);

                // Load cart items if they haven't been loaded
                if (!_context.Entry(cart).Collection(c => c.CartItems).IsLoaded)
                {
                    await _context.Entry(cart)
                        .Collection(c => c.CartItems)
                        .LoadAsync();
                }

                decimal subtotal = 0;
                if (cart.CartItems != null)
                {
                    subtotal = cart.CartItems.Sum(item => item.PriceAtAddition * item.Quantity);
                }

                return new CartSummaryDto
                {
                    CartId = cart.CartId,
                    ItemsCount = cart.CartItems?.Count ?? 0,
                    SubTotal = subtotal,
                    LastUpdated = cart.UpdatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching cart summary for customer");
                throw;
            }
        }

        public async Task<CartItemDto> GetCartItemByIdAsync(int cartItemId)
        {
            try
            {
                var cartItem = await _context.CartItems
                    .Include(ci => ci.Product)
                    .Include(ci => ci.Variant)
                    .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);

                if (cartItem == null)
                    return null;

                var cartItemDto = _mapper.Map<CartItemDto>(cartItem);
                cartItemDto.ProductName = cartItem.Product.Name;
                cartItemDto.ProductImage = cartItem.Product.MainImageUrl;

                if (cartItem.VariantId.HasValue && cartItem.Variant != null)
                {
                    cartItemDto.VariantName = cartItem.Variant.VariantName;
                }

                return cartItemDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching cart item {CartItemId}", cartItemId);
                throw;
            }
        }

        public async Task<CartItemDto> AddCartItemAsync(int userId, AddCartItemDto cartItemDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var customerId = getCutomerId(userId);
                if (customerId == 0)
                {
                    throw new KeyNotFoundException("not found");
                }
                // Get or create cart
                var cart = await GetOrCreateCartAsync(customerId);

                // Load cart items if they haven't been loaded
                if (!_context.Entry(cart).Collection(c => c.CartItems).IsLoaded)
                {
                    await _context.Entry(cart)
                        .Collection(c => c.CartItems)
                        .LoadAsync();
                }

                // Check if product exists
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == cartItemDto.ProductId && p.IsAvailable == true && p.ApprovalStatus != ProductApprovalStatus.Deleted);

                if (product == null)
                {
                    throw new KeyNotFoundException($"Product with ID {cartItemDto.ProductId} not found or is not available");
                }

                // Check variant if specified
                ProductVariant variant = null;
                if (cartItemDto.VariantId.HasValue)
                {
                    variant = await _context.ProductVariants
                        .FirstOrDefaultAsync(v => v.VariantId == cartItemDto.VariantId && v.ProductId == cartItemDto.ProductId && v.IsAvailable == true);

                    if (variant == null)
                    {
                        throw new KeyNotFoundException($"Variant with ID {cartItemDto.VariantId} not found or is not available for this product");
                    }

                    // Check variant stock quantity
                    if (variant.StockQuantity < cartItemDto.Quantity)
                    {
                        throw new InsufficientStockException(
                            cartItemDto.Quantity,
                            variant.StockQuantity
                        );
                    }
                }
                else
                {
                    // Check product stock quantity only if no variant is specified
                    if (product.StockQuantity < cartItemDto.Quantity)
                    {
                        throw new InsufficientStockException(
                            cartItemDto.Quantity,
                            product.StockQuantity
                        );
                    }
                }

                // Check if the item is already in the cart
                var existingItem = cart.CartItems.FirstOrDefault(
                    ci => ci.ProductId == cartItemDto.ProductId &&
                          ci.VariantId == cartItemDto.VariantId);

                CartItem cartItem;

                if (existingItem != null)
                {
                    // Check total quantity after update for stock availability
                    int totalQuantity = existingItem.Quantity + cartItemDto.Quantity;
                    if (cartItemDto.VariantId.HasValue)
                    {
                        if (variant.StockQuantity < totalQuantity)
                        {
                            throw new InsufficientStockException(
                                totalQuantity,
                                variant.StockQuantity
                            );
                        }
                    }
                    else
                    {
                        if (product.StockQuantity < totalQuantity)
                        {
                            throw new InsufficientStockException(
                                totalQuantity,
                                product.StockQuantity
                            );
                        }
                    }

                    // Update quantity
                    existingItem.Quantity = totalQuantity;
                    _context.Entry(existingItem).State = EntityState.Modified;
                    cartItem = existingItem;
                }
                else
                {
                    // Add new item
                    decimal priceToUse = variant != null
                        ? variant.Price - (variant.Price * (variant.DiscountPercentage ?? 0) / 100)
                        : product.BasePrice - (product.BasePrice * (product.DiscountPercentage ?? 0) / 100);

                    cartItem = new CartItem
                    {
                        CartId = cart.CartId,
                        ProductId = cartItemDto.ProductId,
                        Quantity = cartItemDto.Quantity,
                        PriceAtAddition = priceToUse,
                        VariantId = cartItemDto.VariantId
                    };

                    _context.CartItems.Add(cartItem);
                }

                // Update cart timestamp
                cart.UpdatedAt = DateTime.UtcNow;
                _context.Entry(cart).State = EntityState.Modified;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Load product details for the response
                await _context.Entry(cartItem)
                    .Reference(i => i.Product)
                    .LoadAsync();

                if (cartItem.VariantId.HasValue)
                {
                    await _context.Entry(cartItem)
                        .Reference(i => i.Variant)
                        .LoadAsync();
                }

                var cartItemResponse = _mapper.Map<CartItemDto>(cartItem);
                cartItemResponse.ProductName = cartItem.Product.Name;
                cartItemResponse.ProductImage = cartItem.Product.MainImageUrl;

                if (cartItem.VariantId.HasValue && cartItem.Variant != null)
                {
                    cartItemResponse.VariantName = cartItem.Variant.VariantName;
                    cartItemResponse.ProductImage = cartItem.Variant.VariantImageUrl;
                }

                return cartItemResponse;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while adding item to cart for customer");
                throw;
            }
        }
        public async Task<CartItemDto> UpdateCartItemQuantityAsync(int userId, UpdateCartItemDto updateCartItemDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var customerId = getCutomerId(userId);
                if (customerId == 0)
                {
                    throw new KeyNotFoundException("Customer was not found");
                }

                // Get the cart item
                var cartItem = await _context.CartItems
                    .Include(ci => ci.Cart)
                    .Include(ci => ci.Product)
                    .Include(ci => ci.Variant)
                    .FirstOrDefaultAsync(ci => ci.CartItemId == updateCartItemDto.CartItemId);

                if (cartItem == null)
                    throw new KeyNotFoundException($"Cart item with ID {updateCartItemDto.CartItemId} not found");

                // Verify the cart belongs to this customer
                if (cartItem.Cart.CustomerId != customerId)
                    throw new UnauthorizedAccessException("Cart item does not belong to this customer");

                // Check stock quantity based on whether it's a variant or base product
                if (cartItem.VariantId.HasValue && cartItem.Variant != null)
                {
                    if (updateCartItemDto.Quantity > cartItem.Variant.StockQuantity)
                    {
                        throw new InsufficientStockException(
                            updateCartItemDto.Quantity,
                            cartItem.Variant.StockQuantity
                        );
                    }
                }
                else
                {
                    if (updateCartItemDto.Quantity > cartItem.Product.StockQuantity)
                    {
                        throw new InsufficientStockException(
                            updateCartItemDto.Quantity,
                            cartItem.Product.StockQuantity
                        );
                    }
                }

                // Update quantity
                cartItem.Quantity = updateCartItemDto.Quantity;

                // Update cart timestamp
                cartItem.Cart.UpdatedAt = DateTime.UtcNow;

                _context.Entry(cartItem).State = EntityState.Modified;
                _context.Entry(cartItem.Cart).State = EntityState.Modified;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Prepare response
                var cartItemResponse = _mapper.Map<CartItemDto>(cartItem);
                cartItemResponse.ProductName = cartItem.Product.Name;
                cartItemResponse.ProductImage = cartItem.Product.MainImageUrl;

                if (cartItem.VariantId.HasValue && cartItem.Variant != null)
                {
                    cartItemResponse.VariantName = cartItem.Variant.VariantName;
                    cartItemResponse.ProductImage = cartItem.Variant.VariantImageUrl;
                }

                return cartItemResponse;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while updating cart item quantity for customer");
                throw;
            }
        }
        public async Task<bool> RemoveCartItemAsync(int userId, int cartItemId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var customerId = getCutomerId(userId);
                if (customerId == 0) throw new KeyNotFoundException("cartItem was not found");
                // Get the cart item with cart
                var cartItem = await _context.CartItems
                    .Include(ci => ci.Cart)
                    .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);

                if (cartItem == null)
                    return false;

                // Verify the cart belongs to this customer
                if (cartItem.Cart.CustomerId != customerId)
                    throw new UnauthorizedAccessException("Cart item does not belong to this customer");

                // Remove item
                _context.CartItems.Remove(cartItem);

                // Update cart timestamp
                cartItem.Cart.UpdatedAt = DateTime.UtcNow;
                _context.Entry(cartItem.Cart).State = EntityState.Modified;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while removing cart item for customer");
                throw;
            }
        }

        public async Task<bool> ClearCartAsync(int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
            var customerId = getCutomerId(userId);
            if (customerId == 0) throw new KeyNotFoundException("cartItem was not found");
                // Get the cart
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.CustomerId == customerId);

                if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
                    return true; // No items to clear

                // Remove all items
                _context.CartItems.RemoveRange(cart.CartItems);

                // Update cart timestamp
                cart.UpdatedAt = DateTime.UtcNow;
                _context.Entry(cart).State = EntityState.Modified;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while clearing cart for customer");
                throw;
            }
        }

        public async Task<bool> ProductExistsInCartAsync(int customerId, int productId, int? variantId = null)
        {
            try
            {
                // Get cart
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.CustomerId == customerId);

                if (cart == null || cart.CartItems == null)
                    return false;

                // Check if product exists in cart
                return cart.CartItems.Any(ci => ci.ProductId == productId && ci.VariantId == variantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if product exists in cart for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<bool> CartItemExistsAndBelongsToCustomerAsync(int userId, int cartItemId)
        {
            try
            {

                var customerId = getCutomerId(userId);
                if (customerId == 0) throw new KeyNotFoundException("Customer was not found");
                return await _context.CartItems
                    .Include(ci => ci.Cart)
                    .AnyAsync(ci => ci.CartItemId == cartItemId && ci.Cart.CustomerId == customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if cart item exists for customer");
                throw;
            }
        }

        public async Task<int> GetCartItemsCountAsync(int customerId)
        {
            try
            {
                // Get cart
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.CustomerId == customerId);

                if (cart == null || cart.CartItems == null)
                    return 0;

                return cart.CartItems.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart items count for customer {CustomerId}", customerId);
                throw;
            }
        }

        // Helper method to get existing cart or create a new one
        private async Task<Cart> GetOrCreateCartAsync(int customerId)
        {
            // Try to find existing cart
            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            // If no cart exists, create one
            if (cart == null)
            {
                cart = new Cart
                {
                    CustomerId = customerId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }


        public async Task<WishlistItemDto> SaveCartItemForLaterAsync(int customerId, int cartItemId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Get the cart item with product details
                var cartItem = await _context.CartItems
                    .Include(ci => ci.Cart)
                    .Include(ci => ci.Product)
                    .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);

                if (cartItem == null)
                    throw new KeyNotFoundException($"Cart item with ID {cartItemId} not found");

                // Verify the cart belongs to this customer
                if (cartItem.Cart.CustomerId != customerId)
                    throw new UnauthorizedAccessException("Cart item does not belong to this customer");

                // Get or create wishlist
                var wishlist = await _context.Wishlists
                    .FirstOrDefaultAsync(w => w.CustomerId == customerId);

                if (wishlist == null)
                {
                    wishlist = new Wishlist
                    {
                        CustomerId = customerId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Wishlists.Add(wishlist);
                    await _context.SaveChangesAsync();
                }

                // Check if product already exists in wishlist
                var existingWishlistItem = await _context.WishlistItems
                    .FirstOrDefaultAsync(wi => wi.WishlistId == wishlist.WishlistId && wi.ProductId == cartItem.ProductId);

                // If the item is already in wishlist, just remove from cart
                if (existingWishlistItem != null)
                {
                    _context.CartItems.Remove(cartItem);

                    // Update cart timestamp
                    cartItem.Cart.UpdatedAt = DateTime.UtcNow;
                    _context.Entry(cartItem.Cart).State = EntityState.Modified;

                    await _context.SaveChangesAsync();

                    // Return existing wishlist item
                    var product = cartItem.Product;

                    // Calculate current price
                    decimal currentPrice = product.BasePrice;
                    if (product.DiscountPercentage.HasValue && product.DiscountPercentage > 0)
                    {
                        currentPrice = product.BasePrice - (product.BasePrice * product.DiscountPercentage.Value / 100);
                    }

                    var wishlistItemDto = new WishlistItemDto
                    {
                        WishlistItemId = existingWishlistItem.WishlistItemId,
                        WishlistId = existingWishlistItem.WishlistId,
                        ProductId = existingWishlistItem.ProductId,
                        ProductName = product.Name,
                        ProductDescription = product.Description,
                        BasePrice = product.BasePrice,
                        DiscountPercentage = product.DiscountPercentage,
                        CurrentPrice = currentPrice,
                        MainImageUrl = product.MainImageUrl,
                        IsAvailable = product.IsAvailable ?? false,
                        StockQuantity = product.StockQuantity,
                        AddedAt = existingWishlistItem.AddedAt
                    };

                    await transaction.CommitAsync();
                    return wishlistItemDto;
                }

                // Create wishlist item
                var wishlistItem = new WishlistItem
                {
                    WishlistId = wishlist.WishlistId,
                    ProductId = cartItem.ProductId,
                    AddedAt = DateTime.UtcNow
                };

                _context.WishlistItems.Add(wishlistItem);

                // Remove from cart
                _context.CartItems.Remove(cartItem);

                // Update cart timestamp
                cartItem.Cart.UpdatedAt = DateTime.UtcNow;
                _context.Entry(cartItem.Cart).State = EntityState.Modified;

                await _context.SaveChangesAsync();

                // Prepare result
                var prod = cartItem.Product;

                // Calculate current price
                decimal price = prod.BasePrice;
                if (prod.DiscountPercentage.HasValue && prod.DiscountPercentage > 0)
                {
                    price = prod.BasePrice - (prod.BasePrice * prod.DiscountPercentage.Value / 100);
                }

                var result = new WishlistItemDto
                {
                    WishlistItemId = wishlistItem.WishlistItemId,
                    WishlistId = wishlistItem.WishlistId,
                    ProductId = wishlistItem.ProductId,
                    ProductName = prod.Name,
                    ProductDescription = prod.Description,
                    BasePrice = prod.BasePrice,
                    DiscountPercentage = prod.DiscountPercentage,
                    CurrentPrice = price,
                    MainImageUrl = prod.MainImageUrl,
                    IsAvailable = prod.IsAvailable ?? false,
                    StockQuantity = prod.StockQuantity,
                    AddedAt = wishlistItem.AddedAt
                };

                await transaction.CommitAsync();
                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while saving cart item for later for customer {CustomerId}", customerId);
                throw;
            }
        }

        private int getCutomerId(int userId)
        {
            return _context.Customers.FirstOrDefault(c => c.UserId == userId)?.CustomerId ?? 0;
        }
    }
}