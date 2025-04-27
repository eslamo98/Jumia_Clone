using Jumia_Clone.Data;
using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Models.DTOs.ProductVariantDTOs;
using Jumia_Clone.Models.Entities;
using Jumia_Clone.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Jumia_Clone.Repositories.Implementation
{
    public class ProductVariantsRepository : IProductVariantsRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductVariantsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ProductVariant> GetByIdAsync(int variantId)
        {
            return await _context.ProductVariants
                .FirstOrDefaultAsync(v => v.VariantId == variantId);
        }

        public async Task<ProductVariant> GetByIdWithDetailsAsync(int variantId)
        {
            return await _context.ProductVariants
                .Include(v => v.Product)
                .Include(v => v.VariantAttributes)
                .FirstOrDefaultAsync(v => v.VariantId == variantId);
        }

        public async Task<IEnumerable<ProductVariant>> GetAllAsync(int? productId = null)
        {
            var query = _context.ProductVariants.AsQueryable();

            if (productId.HasValue)
            {
                query = query.Where(v => v.ProductId == productId.Value);
            }

            return await query
                .Include(v => v.VariantAttributes)
                .ToListAsync();
        }

        public async Task<ProductVariant> AddAsync(ProductVariant variant)
        {
            // Check if this is the first variant for the product
            bool isFirstVariant = !await _context.ProductVariants
                .AnyAsync(v => v.ProductId == variant.ProductId);

            // If it's the first variant, set it as default
            if (isFirstVariant)
            {
                variant.IsDefault = true;
            }
            else if (variant.IsDefault == true)
            {
                // If this new variant is marked as default, unmark any existing default
                var existingDefault = await _context.ProductVariants
                    .Where(v => v.ProductId == variant.ProductId && v.IsDefault == true)
                    .ToListAsync();

                foreach (var defaultVariant in existingDefault)
                {
                    defaultVariant.IsDefault = false;
                }
            }

            _context.ProductVariants.Add(variant);
            await _context.SaveChangesAsync();

            return variant;
        }

        public async Task<ProductVariant> UpdateAsync(ProductVariant variant, List<UpdateVariantAttributeDto> attributes)
        {
            var existingVariant = await _context.ProductVariants
                .Include(v => v.VariantAttributes)
                .Include(v => v.Product)
                .FirstOrDefaultAsync(v => v.VariantId == variant.VariantId);

            if (existingVariant == null)
            {
                throw new KeyNotFoundException($"Variant with ID {variant.VariantId} not found");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Update variant properties
                existingVariant.VariantName = variant.VariantName;
                existingVariant.Price = variant.Price;
                existingVariant.DiscountPercentage = variant.DiscountPercentage;
                existingVariant.StockQuantity = variant.StockQuantity;
                existingVariant.Sku = variant.Sku;
                existingVariant.IsAvailable = variant.IsAvailable;

                // Handle the default flag
                if (variant.IsDefault == true && existingVariant.IsDefault != true)
                {
                    // If this variant is being set as default, unset any existing default
                    var currentDefault = await _context.ProductVariants
                        .Where(v => v.ProductId == existingVariant.ProductId && v.IsDefault == true && v.VariantId != variant.VariantId)
                        .ToListAsync();

                    foreach (var defaultVariant in currentDefault)
                    {
                        defaultVariant.IsDefault = false;
                    }

                    existingVariant.IsDefault = true;

                    // Sync the product data with this new default variant
                    existingVariant.Product.BasePrice = existingVariant.Price;
                    existingVariant.Product.DiscountPercentage = existingVariant.DiscountPercentage;
                    existingVariant.Product.StockQuantity = existingVariant.StockQuantity;
                }
                else
                {
                    existingVariant.IsDefault = variant.IsDefault;

                    // If this is already the default variant, update product data
                    if (existingVariant.IsDefault == true)
                    {
                        existingVariant.Product.BasePrice = existingVariant.Price;
                        existingVariant.Product.DiscountPercentage = existingVariant.DiscountPercentage;
                        existingVariant.Product.StockQuantity = existingVariant.StockQuantity;
                    }
                }

                // Handle variant attributes if provided
                if (variant.VariantAttributes != null)
                {
                    await SyncVariantAttributesAsync(variant.VariantId, attributes);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return existingVariant;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ProductVariant> UpdateVariantImageAsync(int variantId, string imagePath)
        {
            var variant = await _context.ProductVariants.FindAsync(variantId);
            if (variant == null)
            {
                throw new KeyNotFoundException($"Variant with ID {variantId} not found");
            }

            variant.VariantImageUrl = imagePath;
            await _context.SaveChangesAsync();

            return variant;
        }

        public async Task<bool> DeleteAsync(int variantId)
        {
            var variant = await _context.ProductVariants
                .Include(v => v.OrderItems)
                .Include(v => v.CartItems)
                .Include(v => v.Product)
                .Include(v => v.VariantAttributes)
                .FirstOrDefaultAsync(v => v.VariantId == variantId);

            if (variant == null)
            {
                throw new KeyNotFoundException($"Variant with ID {variantId} not found");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Check if the variant is used in any orders
                if (variant.OrderItems.Any())
                {
                    _context.OrderItems.RemoveRange(variant.OrderItems);
                }

                // Remove from carts
                if (variant.CartItems.Any())
                {
                    _context.CartItems.RemoveRange(variant.CartItems);
                }

                // Remove variant attributes
                if (variant.VariantAttributes.Any())
                {
                    _context.VariantAttributes.RemoveRange(variant.VariantAttributes);
                }

                // Handle default variant changes
                if (variant.IsDefault == true)
                {
                    var otherVariant = await _context.ProductVariants
                        .Where(v => v.ProductId == variant.ProductId && v.VariantId != variantId)
                        .FirstOrDefaultAsync();

                    if (otherVariant != null)
                    {
                        // Make another variant the default and sync product data
                        otherVariant.IsDefault = true;
                        variant.Product.BasePrice = otherVariant.Price;
                        variant.Product.DiscountPercentage = otherVariant.DiscountPercentage;
                        variant.Product.StockQuantity = otherVariant.StockQuantity;
                    }
                    else
                    {
                        // This was the last variant, throw exception
                        throw new InvalidOperationException("Cannot delete the only variant of a product");
                    }
                }

                _context.ProductVariants.Remove(variant);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ProductVariant> UpdateStockAsync(int variantId, int newStock)
        {
            var variant = await _context.ProductVariants
                .Include(v => v.Product)
                .FirstOrDefaultAsync(v => v.VariantId == variantId);

            if (variant == null)
            {
                throw new KeyNotFoundException($"Variant with ID {variantId} not found");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                variant.StockQuantity = newStock;

                // If this is the default variant, sync with product
                if (variant.IsDefault == true)
                {
                    variant.Product.StockQuantity = newStock;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return variant;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DecrementStockAsync(int variantId, int quantity)
        {
            var variant = await _context.ProductVariants
                .Include(v => v.Product)
                .FirstOrDefaultAsync(v => v.VariantId == variantId);

            if (variant == null)
            {
                throw new KeyNotFoundException($"Variant with ID {variantId} not found");
            }

            if (variant.StockQuantity < quantity)
            {
                throw new InvalidOperationException("Not enough stock available");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                variant.StockQuantity -= quantity;

                // If this is the default variant, sync with product
                if (variant.IsDefault == true)
                {
                    variant.Product.StockQuantity = variant.StockQuantity;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<ProductVariant>> GetLowStockVariantsAsync(int threshold = 10)
        {
            return await _context.ProductVariants
                .Include(v => v.Product)
                .Where(v => v.StockQuantity <= threshold && v.IsAvailable == true)
                .OrderBy(v => v.StockQuantity)
                .ToListAsync();
        }

        public async Task<ProductVariant> GetDefaultVariantForProductAsync(int productId)
        {
            return await _context.ProductVariants
                .FirstOrDefaultAsync(v => v.ProductId == productId && v.IsDefault == true);
        }

        public async Task<ProductVariant> SetAsDefaultVariantAsync(int variantId)
        {
            var variant = await _context.ProductVariants
                .Include(v => v.Product)
                .FirstOrDefaultAsync(v => v.VariantId == variantId);

            if (variant == null)
            {
                throw new KeyNotFoundException($"Variant with ID {variantId} not found");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Find and update all variants for this product
                var productVariants = await _context.ProductVariants
                    .Where(v => v.ProductId == variant.ProductId)
                    .ToListAsync();

                foreach (var v in productVariants)
                {
                    v.IsDefault = (v.VariantId == variantId);
                }

                // Sync the product data with the new default variant
                variant.Product.BasePrice = variant.Price;
                variant.Product.DiscountPercentage = variant.DiscountPercentage;
                variant.Product.StockQuantity = variant.StockQuantity;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return variant;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<VariantAttribute>> GetVariantAttributesAsync(int variantId)
        {
            return await _context.VariantAttributes
                .Where(a => a.VariantId == variantId)
                .ToListAsync();
        }

        public async Task<VariantAttribute> AddVariantAttributeAsync(VariantAttribute attribute)
        {
            _context.VariantAttributes.Add(attribute);
            await _context.SaveChangesAsync();
            return attribute;
        }

        public async Task<VariantAttribute> UpdateVariantAttributeAsync(VariantAttribute attribute)
        {
            var existingAttribute = await _context.VariantAttributes.FindAsync(attribute.VariantAttributeId);
            if (existingAttribute == null)
            {
                throw new KeyNotFoundException($"Variant attribute with ID {attribute.VariantAttributeId} not found");
            }

            existingAttribute.AttributeName = attribute.AttributeName;
            existingAttribute.AttributeValue = attribute.AttributeValue;

            await _context.SaveChangesAsync();
            return existingAttribute;
        }

        public async Task<bool> DeleteVariantAttributeAsync(int attributeId)
        {
            var attribute = await _context.VariantAttributes.FindAsync(attributeId);
            if (attribute == null)
            {
                throw new KeyNotFoundException($"Variant attribute with ID {attributeId} not found");
            }

            _context.VariantAttributes.Remove(attribute);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<ProductVariant>> GetVariantsByAttributesAsync(int productId, Dictionary<string, string> attributeFilters)
        {
            // Get all variants for the product
            var variants = await _context.ProductVariants
                .Where(v => v.ProductId == productId)
                .Include(v => v.VariantAttributes)
                .ToListAsync();

            // Filter by attributes
            return variants.Where(variant =>
            {
                foreach (var filter in attributeFilters)
                {
                    string attributeName = filter.Key;
                    string attributeValue = filter.Value;

                    bool hasMatchingAttribute = variant.VariantAttributes.Any(attr =>
                        attr.AttributeName.Equals(attributeName, StringComparison.OrdinalIgnoreCase) &&
                        attr.AttributeValue.Equals(attributeValue, StringComparison.OrdinalIgnoreCase));

                    if (!hasMatchingAttribute)
                    {
                        return false; // This variant doesn't match filter criteria
                    }
                }
                return true; // All filters matched
            });
        }

        public async Task<IEnumerable<ProductVariant>> GetVariantsBySKUAsync(string sku)
        {
            return await _context.ProductVariants
                .Where(v => v.Sku.Contains(sku))
                .Include(v => v.Product)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductVariant>> GetVariantsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            return await _context.ProductVariants
                .Where(v => v.Price >= minPrice && v.Price <= maxPrice)
                .Include(v => v.Product)
                .OrderBy(v => v.Price)
                .ToListAsync();
        }

        public async Task<bool> VariantExistsAsync(int variantId)
        {
            return await _context.ProductVariants.AnyAsync(v => v.VariantId == variantId);
        }

        public async Task<bool> VariantNameExistsForProductAsync(int productId, string variantName, int? excludeVariantId = null)
        {
            var query = _context.ProductVariants
                .Where(v => v.ProductId == productId && v.VariantName == variantName);

            if (excludeVariantId.HasValue)
            {
                query = query.Where(v => v.VariantId != excludeVariantId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<ProductVariant> UpsertVariantAsync(ProductVariant variant, int productId)
        {
            // If the variant has an ID and exists in the database, update it
            if (variant.VariantId > 0 && await VariantExistsAsync(variant.VariantId))
            {
                return await UpdateAsync(variant, new List<UpdateVariantAttributeDto>());
            }

            // Otherwise, it's a new variant, so set the product ID and add it
            variant.ProductId = productId;
            return await AddAsync(variant);
        }

        public async Task SyncVariantAttributesAsync(int variantId, IEnumerable<UpdateVariantAttributeDto> newAttributes)
        {
            // First get the variant with its existing attributes
            var variant = await GetByIdWithDetailsAsync(variantId);
            if (variant == null)
            {
                throw new KeyNotFoundException($"Variant with ID {variantId} not found");
            }

            // If no new attributes provided, do nothing
            if (newAttributes == null || !newAttributes.Any())
            {
                return;
            }

            // Get existing attributes
            var existingAttributes = variant.VariantAttributes.ToList();

            // Identify attributes to delete (existing but not in new list)
            var attributesToDelete = existingAttributes
                .Where(existing => !newAttributes.Any(newAttr =>
                    newAttr.Name.Equals(existing.AttributeName, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            // Delete attributes that aren't in the new list
            foreach (var attrToDelete in attributesToDelete)
            {
                await DeleteVariantAttributeAsync(attrToDelete.VariantAttributeId);
            }

            // For each new attribute
            foreach (var newAttr in newAttributes)
            {
                // Check if it already exists
                var existingAttr = existingAttributes.FirstOrDefault(a =>
                    a.AttributeName.Equals(newAttr.Name, StringComparison.OrdinalIgnoreCase));

                if (existingAttr != null)
                {
                    // Update existing attribute if value is different
                    existingAttr.AttributeValue = newAttr.Value;
                    await UpdateVariantAttributeAsync(existingAttr);
                }
                else
                {
                    // Create new attribute
                    var attribute = new VariantAttribute
                    {
                        VariantId = variantId,
                        AttributeName = newAttr.Name,
                        AttributeValue = newAttr.Value
                    };
                    await AddVariantAttributeAsync(attribute);
                }
            }
        }
    }
}
