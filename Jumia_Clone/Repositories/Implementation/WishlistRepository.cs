using AutoMapper;
using Jumia_Clone.Data;
using Jumia_Clone.Models.DTOs.CartDTOs;
using Jumia_Clone.Models.DTOs.WishlistItemDTOs;
using Jumia_Clone.Models.Entities;
using Jumia_Clone.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Jumia_Clone.Repositories.Implementation
{
    public class WishlistRepository : IWishlistRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<WishlistRepository> _logger;
        private readonly ICartRepository _cartRepository;

        public WishlistRepository(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<WishlistRepository> logger,
            ICartRepository cartRepository)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _cartRepository = cartRepository;
        }

        public async Task<WishlistDto> GetWishlistByCustomerIdAsync(int customerId, bool includeItems = true)
        {
            try
            {
                // Get or create the wishlist
                var wishlist = await GetOrCreateWishlistAsync(customerId);

                // Map to DTO
                var wishlistDto = _mapper.Map<WishlistDto>(wishlist);

                // Get customer name
                var customer = await _context.Customers
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.CustomerId == customerId);

                if (customer != null && customer.User != null)
                {
                    wishlistDto.CustomerName = $"{customer.User.FirstName} {customer.User.LastName}";
                }

                // Include wishlist items if requested
                if (includeItems && wishlist.WishlistItems != null)
                {
                    // Load wishlist items with product information
                    await _context.Entry(wishlist)
                        .Collection(w => w.WishlistItems)
                        .LoadAsync();

                    var wishlistItems = new List<WishlistItemDto>();

                    foreach (var item in wishlist.WishlistItems)
                    {
                        // Load product for this item
                        await _context.Entry(item)
                            .Reference(i => i.Product)
                            .LoadAsync();

                        var product = item.Product;

                        if (product != null)
                        {
                            // Calculate current price
                            decimal currentPrice = product.BasePrice;
                            if (product.DiscountPercentage.HasValue && product.DiscountPercentage > 0)
                            {
                                currentPrice = product.BasePrice - (product.BasePrice * product.DiscountPercentage.Value / 100);
                            }

                            // Create wishlist item DTO
                            var itemDto = new WishlistItemDto
                            {
                                WishlistItemId = item.WishlistItemId,
                                WishlistId = item.WishlistId,
                                ProductId = item.ProductId,
                                ProductName = product.Name,
                                ProductDescription = product.Description,
                                BasePrice = product.BasePrice,
                                DiscountPercentage = product.DiscountPercentage,
                                CurrentPrice = currentPrice,
                                MainImageUrl = product.MainImageUrl,
                                IsAvailable = product.IsAvailable ?? false,
                                StockQuantity = product.StockQuantity,
                                AddedAt = item.AddedAt
                            };

                            wishlistItems.Add(itemDto);
                        }
                    }

                    // Sort by date added (newest first)
                    wishlistItems = wishlistItems.OrderByDescending(i => i.AddedAt).ToList();

                    wishlistDto.WishlistItems = wishlistItems;
                    wishlistDto.ItemsCount = wishlistItems.Count;
                }
                else
                {
                    // Just get the count if items are not included
                    wishlistDto.ItemsCount = await _context.WishlistItems
                        .CountAsync(i => i.WishlistId == wishlist.WishlistId);
                }

                return wishlistDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching wishlist for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<WishlistSummaryDto> GetWishlistSummaryByCustomerIdAsync(int customerId)
        {
            try
            {
                // Get or create the wishlist
                var wishlist = await GetOrCreateWishlistAsync(customerId);

                // Get the latest added date
                var latestItem = await _context.WishlistItems
                    .Where(i => i.WishlistId == wishlist.WishlistId)
                    .OrderByDescending(i => i.AddedAt)
                    .FirstOrDefaultAsync();

                // Get the count
                var itemCount = await _context.WishlistItems
                    .CountAsync(i => i.WishlistId == wishlist.WishlistId);

                return new WishlistSummaryDto
                {
                    WishlistId = wishlist.WishlistId,
                    CustomerId = customerId,
                    ItemsCount = itemCount,
                    LastUpdated = latestItem?.AddedAt ?? wishlist.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching wishlist summary for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<WishlistItemDto> GetWishlistItemByIdAsync(int wishlistItemId)
        {
            try
            {
                var wishlistItem = await _context.WishlistItems
                    .Include(i => i.Product)
                    .FirstOrDefaultAsync(i => i.WishlistItemId == wishlistItemId);

                if (wishlistItem == null)
                    return null;

                var product = wishlistItem.Product;

                // Calculate current price
                decimal currentPrice = product.BasePrice;
                if (product.DiscountPercentage.HasValue && product.DiscountPercentage > 0)
                {
                    currentPrice = product.BasePrice - (product.BasePrice * product.DiscountPercentage.Value / 100);
                }

                return new WishlistItemDto
                {
                    WishlistItemId = wishlistItem.WishlistItemId,
                    WishlistId = wishlistItem.WishlistId,
                    ProductId = wishlistItem.ProductId,
                    ProductName = product.Name,
                    ProductDescription = product.Description,
                    BasePrice = product.BasePrice,
                    DiscountPercentage = product.DiscountPercentage,
                    CurrentPrice = currentPrice,
                    MainImageUrl = product.MainImageUrl,
                    IsAvailable = product.IsAvailable ?? false,
                    StockQuantity = product.StockQuantity,
                    AddedAt = wishlistItem.AddedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching wishlist item {WishlistItemId}", wishlistItemId);
                throw;
            }
        }

        public async Task<bool> WishlistItemExistsAsync(int customerId, int productId)
        {
            try
            {
                // Get the wishlist ID for this customer
                var wishlist = await _context.Wishlists
                    .FirstOrDefaultAsync(w => w.CustomerId == customerId);

                if (wishlist == null)
                    return false;

                // Check if product exists in the wishlist
                return await _context.WishlistItems
                    .AnyAsync(i => i.WishlistId == wishlist.WishlistId && i.ProductId == productId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if product {ProductId} exists in wishlist for customer {CustomerId}",
                    productId, customerId);
                throw;
            }
        }

        public async Task<bool> WishlistItemBelongsToCustomerAsync(int customerId, int wishlistItemId)
        {
            try
            {
                return await _context.WishlistItems
                    .Include(i => i.Wishlist)
                    .AnyAsync(i => i.WishlistItemId == wishlistItemId && i.Wishlist.CustomerId == customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if wishlist item {WishlistItemId} belongs to customer {CustomerId}",
                    wishlistItemId, customerId);
                throw;
            }
        }

        public async Task<WishlistItemDto> AddItemToWishlistAsync(int customerId, AddWishlistItemDto itemDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Get or create the wishlist
                var wishlist = await GetOrCreateWishlistAsync(customerId);

                // Check if the product exists
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == itemDto.ProductId);

                if (product == null)
                {
                    throw new KeyNotFoundException($"Product with ID {itemDto.ProductId} not found");
                }

                // Check if this product is already in the wishlist
                var existingItem = await _context.WishlistItems
                    .FirstOrDefaultAsync(i => i.WishlistId == wishlist.WishlistId && i.ProductId == itemDto.ProductId);

                if (existingItem != null)
                {
                    // Item already exists, just update the timestamp
                    existingItem.AddedAt = DateTime.UtcNow;
                    _context.Entry(existingItem).State = EntityState.Modified;
                    await _context.SaveChangesAsync();

                    // Calculate current price
                    decimal currentPrice = product.BasePrice;
                    if (product.DiscountPercentage.HasValue && product.DiscountPercentage > 0)
                    {
                        currentPrice = product.BasePrice - (product.BasePrice * product.DiscountPercentage.Value / 100);
                    }

                    var itemDto2 = new WishlistItemDto
                    {
                        WishlistItemId = existingItem.WishlistItemId,
                        WishlistId = existingItem.WishlistId,
                        ProductId = existingItem.ProductId,
                        ProductName = product.Name,
                        ProductDescription = product.Description,
                        BasePrice = product.BasePrice,
                        DiscountPercentage = product.DiscountPercentage,
                        CurrentPrice = currentPrice,
                        MainImageUrl = product.MainImageUrl,
                        IsAvailable = product.IsAvailable ?? false,
                        StockQuantity = product.StockQuantity,
                        AddedAt = existingItem.AddedAt
                    };

                    await transaction.CommitAsync();
                    return itemDto2;
                }

                // Add new item to wishlist
                var wishlistItem = new WishlistItem
                {
                    WishlistId = wishlist.WishlistId,
                    ProductId = itemDto.ProductId,
                    AddedAt = DateTime.UtcNow
                };

                _context.WishlistItems.Add(wishlistItem);
                await _context.SaveChangesAsync();

                // Calculate current price
                decimal price = product.BasePrice;
                if (product.DiscountPercentage.HasValue && product.DiscountPercentage > 0)
                {
                    price = product.BasePrice - (product.BasePrice * product.DiscountPercentage.Value / 100);
                }

                var result = new WishlistItemDto
                {
                    WishlistItemId = wishlistItem.WishlistItemId,
                    WishlistId = wishlistItem.WishlistId,
                    ProductId = wishlistItem.ProductId,
                    ProductName = product.Name,
                    ProductDescription = product.Description,
                    BasePrice = product.BasePrice,
                    DiscountPercentage = product.DiscountPercentage,
                    CurrentPrice = price,
                    MainImageUrl = product.MainImageUrl,
                    IsAvailable = product.IsAvailable ?? false,
                    StockQuantity = product.StockQuantity,
                    AddedAt = wishlistItem.AddedAt
                };

                await transaction.CommitAsync();
                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while adding product {ProductId} to wishlist for customer {CustomerId}",
                    itemDto.ProductId, customerId);
                throw;
            }
        }

        public async Task<bool> RemoveItemFromWishlistAsync(int wishlistItemId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var wishlistItem = await _context.WishlistItems
                    .FirstOrDefaultAsync(i => i.WishlistItemId == wishlistItemId);

                if (wishlistItem == null)
                    return false;

                _context.WishlistItems.Remove(wishlistItem);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while removing wishlist item {WishlistItemId}", wishlistItemId);
                throw;
            }
        }

        public async Task<bool> ClearWishlistAsync(int customerId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Get the wishlist
                var wishlist = await _context.Wishlists
                    .Include(w => w.WishlistItems)
                    .FirstOrDefaultAsync(w => w.CustomerId == customerId);

                if (wishlist == null || wishlist.WishlistItems == null || !wishlist.WishlistItems.Any())
                    return true; // No items to clear

                // Remove all items
                _context.WishlistItems.RemoveRange(wishlist.WishlistItems);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while clearing wishlist for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<CartItemDto> MoveItemToCartAsync(int customerId, int wishlistItemId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Get the wishlist item with product
                var wishlistItem = await _context.WishlistItems
                    .Include(i => i.Product)
                    .FirstOrDefaultAsync(i => i.WishlistItemId == wishlistItemId);

                if (wishlistItem == null)
                {
                    throw new KeyNotFoundException($"Wishlist item with ID {wishlistItemId} not found");
                }

                // Check if product is available
                var product = wishlistItem.Product;
                if (product == null || product.IsAvailable != true || product.StockQuantity <= 0)
                {
                    throw new InvalidOperationException("Product is not available or out of stock");
                }

                // Add to cart
                var addCartItemDto = new AddCartItemDto
                {
                    ProductId = wishlistItem.ProductId,
                    Quantity = 1
                };

                var cartItem = await _cartRepository.AddCartItemAsync(customerId, addCartItemDto);

                // Remove from wishlist
                _context.WishlistItems.Remove(wishlistItem);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return cartItem;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while moving wishlist item {WishlistItemId} to cart for customer {CustomerId}",
                    wishlistItemId, customerId);
                throw;
            }
        }

        public async Task<List<CartItemDto>> MoveMultipleItemsToCartAsync(int customerId, List<int> wishlistItemIds)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var cartItems = new List<CartItemDto>();
                var itemsToRemove = new List<WishlistItem>();

                foreach (var wishlistItemId in wishlistItemIds)
                {
                    // Get the wishlist item with product
                    var wishlistItem = await _context.WishlistItems
                        .Include(i => i.Product)
                        .FirstOrDefaultAsync(i => i.WishlistItemId == wishlistItemId);

                    if (wishlistItem == null)
                    {
                        continue; // Skip if not found
                    }

                    // Check if product is available
                    var product = wishlistItem.Product;
                    if (product == null || product.IsAvailable != true || product.StockQuantity <= 0)
                    {
                        continue; // Skip if not available
                    }

                    // Add to cart
                    try
                    {
                        var addCartItemDto = new AddCartItemDto
                        {
                            ProductId = wishlistItem.ProductId,
                            Quantity = 1
                        };

                        var cartItem = await _cartRepository.AddCartItemAsync(customerId, addCartItemDto);
                        cartItems.Add(cartItem);

                        // Mark for removal from wishlist
                        itemsToRemove.Add(wishlistItem);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error adding product {ProductId} to cart, skipping", wishlistItem.ProductId);
                        // Continue with other items even if one fails
                    }
                }

                // Remove successful items from wishlist
                if (itemsToRemove.Any())
                {
                    _context.WishlistItems.RemoveRange(itemsToRemove);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return cartItems;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while moving multiple wishlist items to cart for customer {CustomerId}",
                    customerId);
                throw;
            }
        }

        public async Task<List<WishlistItemDto>> GetOutOfStockWishlistItemsAsync(int customerId)
        {
            try
            {
                // Get the wishlist
                var wishlist = await _context.Wishlists
                    .FirstOrDefaultAsync(w => w.CustomerId == customerId);

                if (wishlist == null)
                    return new List<WishlistItemDto>();

                // Get items that are out of stock
                var outOfStockItems = await _context.WishlistItems
                    .Include(i => i.Product)
                    .Where(i => i.WishlistId == wishlist.WishlistId &&
                           (i.Product.IsAvailable != true || i.Product.StockQuantity <= 0))
                    .ToListAsync();

                var result = new List<WishlistItemDto>();

                foreach (var item in outOfStockItems)
                {
                    var product = item.Product;

                    // Calculate current price
                    decimal currentPrice = product.BasePrice;
                    if (product.DiscountPercentage.HasValue && product.DiscountPercentage > 0)
                    {
                        currentPrice = product.BasePrice - (product.BasePrice * product.DiscountPercentage.Value / 100);
                    }

                    result.Add(new WishlistItemDto
                    {
                        WishlistItemId = item.WishlistItemId,
                        WishlistId = item.WishlistId,
                        ProductId = item.ProductId,
                        ProductName = product.Name,
                        ProductDescription = product.Description,
                        BasePrice = product.BasePrice,
                        DiscountPercentage = product.DiscountPercentage,
                        CurrentPrice = currentPrice,
                        MainImageUrl = product.MainImageUrl,
                        IsAvailable = product.IsAvailable ?? false,
                        StockQuantity = product.StockQuantity,
                        AddedAt = item.AddedAt
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching out of stock wishlist items for customer {CustomerId}",
                    customerId);
                throw;
            }
        }

        public async Task<List<WishlistItemDto>> GetRecentlyAddedWishlistItemsAsync(int customerId, int count)
        {
            try
            {
                // Get the wishlist
                var wishlist = await _context.Wishlists
                    .FirstOrDefaultAsync(w => w.CustomerId == customerId);

                if (wishlist == null)
                    return new List<WishlistItemDto>();

                // Get recently added items
                var recentItems = await _context.WishlistItems
                    .Include(i => i.Product)
                    .Where(i => i.WishlistId == wishlist.WishlistId)
                    .OrderByDescending(i => i.AddedAt)
                    .Take(count)
                    .ToListAsync();

                var result = new List<WishlistItemDto>();

                foreach (var item in recentItems)
                {
                    var product = item.Product;

                    // Calculate current price
                    decimal currentPrice = product.BasePrice;
                    if (product.DiscountPercentage.HasValue && product.DiscountPercentage > 0)
                    {
                        currentPrice = product.BasePrice - (product.BasePrice * product.DiscountPercentage.Value / 100);
                    }

                    result.Add(new WishlistItemDto
                    {
                        WishlistItemId = item.WishlistItemId,
                        WishlistId = item.WishlistId,
                        ProductId = item.ProductId,
                        ProductName = product.Name,
                        ProductDescription = product.Description,
                        BasePrice = product.BasePrice,
                        DiscountPercentage = product.DiscountPercentage,
                        CurrentPrice = currentPrice,
                        MainImageUrl = product.MainImageUrl,
                        IsAvailable = product.IsAvailable ?? false,
                        StockQuantity = product.StockQuantity,
                        AddedAt = item.AddedAt
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching recently added wishlist items for customer {CustomerId}",
                    customerId);
                throw;
            }
        }

        // Helper method to get or create wishlist
        private async Task<Wishlist> GetOrCreateWishlistAsync(int customerId)
        {
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

            return wishlist;
        }
    }
}