using Jumia_Clone.Data;
using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Models.DTOs.ProductVariantDTOs;
using Jumia_Clone.Models.Entities;
using Jumia_Clone.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Jumia_Clone.Repositories.Implementation
{
    public class ProductVariantRepository : IProductVariantRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductVariantRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all variants for a product
        public async Task<IEnumerable<ProductVariantDto>> GetAllVariantsByProductIdAsync(int productId, PaginationDto pagination)
        {
            // First check if the product exists
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                throw new KeyNotFoundException($"Product with ID {productId} not found");

            var variants = await _context.ProductVariants
                .Where(v => v.ProductId == productId)
                .Skip(pagination.PageSize * pagination.PageNumber)
                .Take(pagination.PageSize)
                .Select(v => new ProductVariantDto
                {
                    VariantId = v.VariantId,
                    VariantName = v.VariantName,
                    Price = v.Price,
                    DiscountPercentage = v.DiscountPercentage ?? 0,
                    StockQuantity = v.StockQuantity,
                    SKU = v.Sku,
                    VariantImageUrl = v.VariantImageUrl,
                    IsDefault = v.IsDefault ?? false,
                    Attributes = v.VariantAttributes.Select(a => new VariantAttributeDto
                    {
                        AttributeName = a.AttributeName,
                        AttributeValue = a.AttributeValue
                    }).ToList()
                })
                .ToListAsync();

            return variants;
        }

        // Get a specific variant by ID
        public async Task<ProductVariantDto> GetVariantByIdAsync(int productId, int variantId)
        {
            var variant = await _context.ProductVariants
                .Where(v => v.ProductId == productId && v.VariantId == variantId)
                .Select(v => new ProductVariantDto
                {
                    VariantId = v.VariantId,
                    VariantName = v.VariantName,
                    Price = v.Price,
                    DiscountPercentage = v.DiscountPercentage ?? 0,
                    StockQuantity = v.StockQuantity,
                    SKU = v.Sku,
                    VariantImageUrl = v.VariantImageUrl,
                    IsDefault = v.IsDefault ?? false,
                    Attributes = v.VariantAttributes.Select(a => new VariantAttributeDto
                    {
                        AttributeName = a.AttributeName,
                        AttributeValue = a.AttributeValue
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (variant == null)
                throw new KeyNotFoundException($"Variant with ID {variantId} not found for product {productId}");

            return variant;
        }

        // Create a new variant
        public async Task<ProductVariantDto> CreateVariantAsync(int productId, CreateProductVariantDto variantDto)
        {
            // Check if the product exists
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                throw new KeyNotFoundException($"Product with ID {productId} not found");

            // If this is set as default, unset any existing default
            if (variantDto.IsDefault)
            {
                var existingDefaultVariants = await _context.ProductVariants
                    .Where(v => v.ProductId == productId && v.IsDefault == true)
                    .ToListAsync();

                foreach (var defaultVariant in existingDefaultVariants)
                {
                    defaultVariant.IsDefault = false;
                }
            }

            // Create new variant
            var variant = new ProductVariant
            {
                ProductId = productId,
                VariantName = variantDto.VariantName,
                Price = variantDto.Price,
                DiscountPercentage = variantDto.DiscountPercentage,
                StockQuantity = variantDto.StockQuantity,
                Sku = variantDto.SKU,
                VariantImageUrl = variantDto.VariantImageUrl,
                IsDefault = variantDto.IsDefault
            };

            _context.ProductVariants.Add(variant);
            await _context.SaveChangesAsync();

            // Add variant attributes
            if (variantDto.Attributes != null && variantDto.Attributes.Any())
            {
                foreach (var attributeDto in variantDto.Attributes)
                {
                    var attribute = new VariantAttribute
                    {
                        VariantId = variant.VariantId,
                        AttributeName = attributeDto.AttributeName,
                        AttributeValue = attributeDto.AttributeValue
                    };
                    _context.VariantAttributes.Add(attribute);
                }
                await _context.SaveChangesAsync();
            }

            // Return the created variant
            return new ProductVariantDto
            {
                VariantId = variant.VariantId,
                VariantName = variant.VariantName,
                Price = variant.Price,
                DiscountPercentage = variant.DiscountPercentage ?? 0,
                StockQuantity = variant.StockQuantity,
                SKU = variant.Sku,
                VariantImageUrl = variant.VariantImageUrl,
                IsDefault = variant.IsDefault ?? false,
                Attributes = variantDto.Attributes?.ToList() ?? new List<VariantAttributeDto>()
            };
        }

        // Update an existing variant
        public async Task<ProductVariantDto> UpdateVariantAsync(int productId, UpdateProductVariantDto variantDto)
        {
            // Ensure the variant exists and belongs to the specified product
            var variant = await _context.ProductVariants
                .Include(v => v.VariantAttributes)
                .FirstOrDefaultAsync(v => v.ProductId == productId && v.VariantId == variantDto.VariantId);

            if (variant == null)
                throw new KeyNotFoundException($"Variant with ID {variantDto.VariantId} not found for product {productId}");

            // If this is set as default, unset any existing default
            if (variantDto.IsDefault && variant.IsDefault != true)
            {
                var existingDefaultVariants = await _context.ProductVariants
                    .Where(v => v.ProductId == productId && v.IsDefault == true)
                    .ToListAsync();

                foreach (var defaultVariant in existingDefaultVariants)
                {
                    defaultVariant.IsDefault = false;
                }
            }

            // Update variant properties
            variant.VariantName = variantDto.VariantName;
            variant.Price = variantDto.Price;
            variant.DiscountPercentage = variantDto.DiscountPercentage;
            variant.StockQuantity = variantDto.StockQuantity;
            variant.Sku = variantDto.SKU;
            variant.VariantImageUrl = variantDto.VariantImageUrl;
            variant.IsDefault = variantDto.IsDefault;

            // Handle attributes - remove existing and add new ones
            _context.VariantAttributes.RemoveRange(variant.VariantAttributes);

            if (variantDto.Attributes != null && variantDto.Attributes.Any())
            {
                foreach (var attributeDto in variantDto.Attributes)
                {
                    var attribute = new VariantAttribute
                    {
                        VariantId = variant.VariantId,
                        AttributeName = attributeDto.AttributeName,
                        AttributeValue = attributeDto.AttributeValue
                    };
                    _context.VariantAttributes.Add(attribute);
                }
            }

            await _context.SaveChangesAsync();

            // Return the updated variant
            return new ProductVariantDto
            {
                VariantId = variant.VariantId,
                VariantName = variant.VariantName,
                Price = variant.Price,
                DiscountPercentage = variant.DiscountPercentage ?? 0,
                StockQuantity = variant.StockQuantity,
                SKU = variant.Sku,
                VariantImageUrl = variant.VariantImageUrl,
                IsDefault = variant.IsDefault ?? false,
                Attributes = variantDto.Attributes?.ToList() ?? new List<VariantAttributeDto>()
            };
        }

        // Delete a variant
        public async Task DeleteVariantAsync(int productId, int variantId)
        {
            // Check if this is the last variant
            bool isLast = await IsLastVariantAsync(productId, variantId);
            if (isLast)
                throw new InvalidOperationException("Cannot delete the last variant of a product");

            var variant = await _context.ProductVariants
                .Include(v => v.VariantAttributes)
                .FirstOrDefaultAsync(v => v.ProductId == productId && v.VariantId == variantId);

            if (variant == null)
                throw new KeyNotFoundException($"Variant with ID {variantId} not found for product {productId}");

            bool wasDefault = variant.IsDefault == true;

            // Remove the variant attributes first
            _context.VariantAttributes.RemoveRange(variant.VariantAttributes);

            // Remove the variant
            _context.ProductVariants.Remove(variant);
            await _context.SaveChangesAsync();

            // If this was the default variant, set a new default
            if (wasDefault)
            {
                await SetNewDefaultVariantAsync(productId, variantId);
            }
        }

       

        // Check if this is the only variant for the product
        public async Task<bool> IsLastVariantAsync(int productId, int variantId)
        {
            int variantCount = await _context.ProductVariants
                .Where(v => v.ProductId == productId)
                .CountAsync();

            return variantCount <= 1;
        }

        // Set a new default variant if the current default is deleted
        public async Task SetNewDefaultVariantAsync(int productId, int excludeVariantId)
        {
            var newDefaultVariant = await _context.ProductVariants
                .Where(v => v.ProductId == productId && v.VariantId != excludeVariantId)
                .FirstOrDefaultAsync();

            if (newDefaultVariant != null)
            {
                newDefaultVariant.IsDefault = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
