using Jumia_Clone.Data;
using Jumia_Clone.Helpers;
using Jumia_Clone.Models.Constants;
using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Models.DTOs.ProductAttributeValueDTOs;
using Jumia_Clone.Models.DTOs.ProductDTOs;
using Jumia_Clone.Models.DTOs.ProductImageDTOs;
using Jumia_Clone.Models.DTOs.ProductVariantDTOs2;
using Jumia_Clone.Models.DTOs.VariantAttributeDTOs;
using Jumia_Clone.Models.Entities;
using Jumia_Clone.Repositories.Interfaces;
using Jumia_Clone.Services.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace Jumia_Clone.Repositories.Implementation
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;

        public ProductRepository(ApplicationDbContext context, IImageService imageService)
        {
            _context = context;
            _imageService = imageService;
        }

        // Get a product by ID with optional detailed information
        public async Task<ProductDto> GetProductByIdAsync(int id, bool includeDetails = false)
        {
            var query = _context.Products
                .Where(p => p.ProductId == id && p.ApprovalStatus != ProductApprovalStatus.Deleted);

            if (includeDetails)
            {
                query = query
                    .Include(p => p.Seller)
                    .Include(p => p.Subcategory)
                    .ThenInclude(sc => sc.Category)
                    .Include(p => p.ProductImages)
                    .Include(p => p.ProductVariants)
                    .Include(p => p.ProductAttributeValues)
                    .ThenInclude(av => av.Attribute)
                    .Include(p => p.Ratings);
            }

            var product = await query.FirstOrDefaultAsync();

            if (product == null)
                return null;

            var productDto = MapToProductDto(product, includeDetails);
            return productDto;
        }

        // Get all products with pagination and filtering
        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync(PaginationDto pagination, ProductFilterDto filter = null)
        {
            filter ??= new ProductFilterDto(); // Initialize empty filter if null

            var query = _context.Products.Where(p => p.ApprovalStatus != ProductApprovalStatus.Deleted)
                .AsQueryable();

            // Apply filters
            if (filter.CategoryId.HasValue)
            {
                query = query.Where(p => p.Subcategory.CategoryId == filter.CategoryId);
            }

            if (filter.SubcategoryId.HasValue)
            {
                query = query.Where(p => p.SubcategoryId == filter.SubcategoryId);
            }

            if (filter.MinPrice.HasValue)
            {
                query = query.Where(p => p.BasePrice >= filter.MinPrice);
            }

            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(p => p.BasePrice <= filter.MaxPrice);
            }

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(p => p.Name.Contains(filter.SearchTerm) || p.Description.Contains(filter.SearchTerm));
            }

            // Filter by approval status
            if (!string.IsNullOrEmpty(filter.ApprovalStatus))
            {
                query = query.Where(p => p.ApprovalStatus == filter.ApprovalStatus);
            }
            else
            //{
            //    // By default, show only approved products
            //    query = query.Where(p => p.ApprovalStatus == ProductApprovalStatus.Approved && p.IsAvailable == true);
            //}

            // Apply sorting
            query = ApplySorting(query, filter.SortBy, filter.SortDirection);

            // Apply pagination
            var products = await query
                .Skip(pagination.PageSize * pagination.PageNumber)
                .Take(pagination.PageSize)
                .Include(p => p.Seller)
                .Include(p => p.Subcategory)
                .Include(p => p.Ratings)
                .Include(p => p.ProductVariants)
                .Include(P => P.ProductImages)
                .Include(p => p.ProductAttributeValues)
                .ThenInclude(p => p.Attribute)
                .ToListAsync();

            return products.Select(p => MapToProductDto(p, true));
        }

        // Create a new product (with pending approval status)
        public async Task<ProductDto> CreateProductAsync(CreateProductInputDto productDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Create the product entity
                var product = new Product
                {
                    Name = productDto.Name,
                    Description = productDto.Description,
                    BasePrice = productDto.BasePrice,
                    DiscountPercentage = productDto.DiscountPercentage,
                    StockQuantity = productDto.StockQuantity,
                    SubcategoryId = productDto.SubcategoryId,
                    SellerId = productDto.SellerId,
                    MainImageUrl = "",
                    IsAvailable = false,

                    ApprovalStatus = ProductApprovalStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // Process attribute values if any
                if (productDto.AttributeValues != null && productDto.AttributeValues.Any())
                {
                    foreach (var attrValue in productDto.AttributeValues)
                    {
                        attrValue.ProductId = product.ProductId;
                        // Validate the attribute exists
                        var attribute = await _context.ProductAttributes
                            .FirstOrDefaultAsync(pa => pa.AttributeId == attrValue.AttributeId);

                        if (attribute == null)
                            throw new KeyNotFoundException($"Attribute with ID {attrValue.AttributeId} not found");

                        // Validate the attribute value
                        ValidateAttributeValue(attribute, attrValue.Value);

                        var attributeValue = new ProductAttributeValue
                        {
                            ProductId = product.ProductId,
                            AttributeId = attrValue.AttributeId,
                            Value = attrValue.Value
                        };

                        _context.ProductAttributeValues.Add(attributeValue);
                    }

                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                // Return the created product
                return await GetProductByIdAsync(product.ProductId, true);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // Add this validation method to your repository
        private void ValidateAttributeValue(ProductAttribute attribute, string value)
        {
            // Check if value is provided
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"Value cannot be empty for attribute {attribute.Name}");

            // Validate based on attribute type
            switch (attribute.Type)
            {
                case ProductAttributeTypes.Text:
                    if (value.Length > 500)
                        throw new ArgumentException($"Text value for {attribute.Name} is too long");
                    break;

                case ProductAttributeTypes.Number:
                    if (!AttributeValidationHelper.IsValidNumeric(value))
                        throw new ArgumentException($"Value for {attribute.Name} must be a valid integer");
                    break;

                case ProductAttributeTypes.Decimal:
                    if (!AttributeValidationHelper.IsValidNumeric(value, "decimal"))
                        throw new ArgumentException($"Value for {attribute.Name} must be a valid decimal number");
                    break;

                case ProductAttributeTypes.Date:
                    if (!AttributeValidationHelper.IsValidDate(value))
                        throw new ArgumentException($"Value for {attribute.Name} must be a valid date");
                    break;

                case ProductAttributeTypes.Dropdown:
                case ProductAttributeTypes.Radio:
                    if (!AttributeValidationHelper.IsValidListValue(value, attribute.PossibleValues))
                        throw new ArgumentException($"Value for {attribute.Name} must be one of: {attribute.PossibleValues}");
                    break;

                case ProductAttributeTypes.Multiselect:
                    if (!AttributeValidationHelper.IsValidListValue(value, attribute.PossibleValues, true))
                        throw new ArgumentException($"All values for {attribute.Name} must be from: {attribute.PossibleValues}");
                    break;

                case ProductAttributeTypes.Checkbox:
                    if (!AttributeValidationHelper.IsValidBoolean(value))
                        throw new ArgumentException($"Value for {attribute.Name} must be true or false");
                    break;

                case ProductAttributeTypes.Color:
                    if (!AttributeValidationHelper.IsValidColor(value))
                        throw new ArgumentException($"Invalid color value for {attribute.Name}");
                    break;

                case ProductAttributeTypes.Size:
                    if (!AttributeValidationHelper.IsValidSize(value, attribute.PossibleValues))
                        throw new ArgumentException($"Invalid size value for {attribute.Name}");
                    break;

                default:
                    throw new ArgumentException($"Unsupported attribute type: {attribute.Type}");
            }
        }

        // Update a product
        public async Task<ProductDto> UpdateProductAsync(int id, UpdateProductInputDto productDto)
        {
            var product = await _context.Products
                .Include(p => p.ProductVariants)
                .Include(p => p.ProductAttributeValues)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
                throw new KeyNotFoundException($"Product with ID {id} not found");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Update basic product information
                product.Name = productDto.Name;
                product.Description = productDto.Description;
                product.BasePrice = productDto.BasePrice;
                product.DiscountPercentage = productDto.DiscountPercentage;
                product.StockQuantity = productDto.StockQuantity;
                product.SubcategoryId = productDto.SubcategoryId;
                product.UpdatedAt = DateTime.UtcNow;

                // If product was approved and seller makes significant changes, reset to pending
                if (product.ApprovalStatus == ProductApprovalStatus.Approved &&
                    (product.Name != productDto.Name ||
                     product.Description != productDto.Description ||
                     product.SubcategoryId != productDto.SubcategoryId))
                {
                    
                    product.ApprovalStatus = ProductApprovalStatus.PendingReview;
                    product.IsAvailable = false;
                }

                // Process attribute values if provided
                if (productDto.ProductAttributeValuesJson != null)
                {
                    // Deserialize attribute values
                    List<CreateProductAttributeValueDto> attributeValues;
                    try
                    {
                        attributeValues = System.Text.Json.JsonSerializer.Deserialize<List<CreateProductAttributeValueDto>>(
                            productDto.ProductAttributeValuesJson,
                            new System.Text.Json.JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            }
                        );
                    }
                    catch (System.Text.Json.JsonException ex)
                    {
                        throw new ArgumentException("Invalid attribute values JSON", ex);
                    }

                    // Remove existing attribute values
                    _context.ProductAttributeValues.RemoveRange(product.ProductAttributeValues);

                    // Add new attribute values
                    if (attributeValues != null && attributeValues.Any())
                    {
                        foreach (var attrValue in attributeValues)
                        {
                            // Validate the attribute exists
                            var attribute = await _context.ProductAttributes
                                .FirstOrDefaultAsync(pa => pa.AttributeId == attrValue.AttributeId);

                            if (attribute == null)
                                throw new KeyNotFoundException($"Attribute with ID {attrValue.AttributeId} not found");

                            // Validate the attribute value
                            ValidateAttributeValue(attribute, attrValue.Value);

                            var productAttributeValue = new ProductAttributeValue
                            {
                                ProductId = product.ProductId,
                                AttributeId = attrValue.AttributeId,
                                Value = attrValue.Value
                            };

                            _context.ProductAttributeValues.Add(productAttributeValue);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Return the updated product
                return await GetProductByIdAsync(id, true);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // Delete a product and all related data
        //public async Task DeleteProductAsync(int id)
        //{
        //    using var transaction = await _context.Database.BeginTransactionAsync();

        //    try
        //    {
        //        // Get product with all related entities
        //        var product = await _context.Products
        //            .Include(p => p.ProductImages)
        //            .Include(p => p.ProductVariants)
        //            .ThenInclude(v => v.VariantAttributes)
        //            .Include(p => p.ProductAttributeValues)
        //            .FirstOrDefaultAsync(p => p.ProductId == id);

        //        if (product == null)
        //            throw new KeyNotFoundException($"Product with ID {id} not found");

        //        // Delete all product images
        //        foreach (var image in product.ProductImages)
        //        {
        //            await _imageService.DeleteImageAsync(image.ImageUrl);
        //            _context.ProductImages.Remove(image);
        //        }

        //        // Delete main product image
        //        if (!string.IsNullOrEmpty(product.MainImageUrl))
        //        {
        //            await _imageService.DeleteImageAsync(product.MainImageUrl);
        //        }

        //        // Delete variant attributes
        //        foreach (var variant in product.ProductVariants)
        //        {
        //            _context.VariantAttributes.RemoveRange(variant.VariantAttributes);
        //        }

        //        // Delete variants
        //        _context.ProductVariants.RemoveRange(product.ProductVariants);

        //        // Delete attribute values
        //        _context.ProductAttributeValues.RemoveRange(product.ProductAttributeValues);

        //        // Check if product is in any carts and remove
        //        var cartItems = await _context.CartItems
        //            .Where(ci => ci.ProductId == id)
        //            .ToListAsync();
        //        _context.CartItems.RemoveRange(cartItems);

        //        // Check if product is in any wishlists and remove
        //        var wishlistItems = await _context.WishlistItems
        //            .Where(wi => wi.ProductId == id)
        //            .ToListAsync();
        //        _context.WishlistItems.RemoveRange(wishlistItems);

        //        // Remove product from trending products
        //        var trendingProducts = await _context.TrendingProducts
        //            .Where(tp => tp.ProductId == id)
        //            .ToListAsync();
        //        _context.TrendingProducts.RemoveRange(trendingProducts);

        //        // Remove product from recommendations
        //        var productRecommendations = await _context.ProductRecommendations
        //            .Where(pr => pr.SourceProductId == id || pr.RecommendedProductId == id)
        //            .ToListAsync();
        //        _context.ProductRecommendations.RemoveRange(productRecommendations);

        //        // Remove user recommendations for this product
        //        var userRecommendations = await _context.UserRecommendations
        //            .Where(ur => ur.ProductId == id)
        //            .ToListAsync();
        //        _context.UserRecommendations.RemoveRange(userRecommendations);

        //        // Remove search result clicks
        //        var searchResultClicks = await _context.SearchResultClicks
        //            .Where(src => src.ProductId == id)
        //            .ToListAsync();
        //        _context.SearchResultClicks.RemoveRange(searchResultClicks);

        //        // Remove product views
        //        var productViews = await _context.ProductViews
        //            .Where(pv => pv.ProductId == id)
        //            .ToListAsync();
        //        _context.ProductViews.RemoveRange(productViews);

        //        // Remove user product interactions
        //        var userProductInteractions = await _context.UserProductInteractions
        //            .Where(upi => upi.ProductId == id)
        //            .ToListAsync();
        //        _context.UserProductInteractions.RemoveRange(userProductInteractions);

        //        // Finally, delete the product
        //        _context.Products.Remove(product);

        //        await _context.SaveChangesAsync();
        //        await transaction.CommitAsync();
        //    }
        //    catch (Exception)
        //    {
        //        await transaction.RollbackAsync();
        //        throw;
        //    }
        //}
        
        // Update the delete method to use soft delete approach
        public async Task DeleteProductAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Get product with its variants
                var product = await _context.Products
                    .Include(p => p.ProductVariants)
                    .FirstOrDefaultAsync(p => p.ProductId == id);

                if (product == null)
                    throw new KeyNotFoundException($"Product with ID {id} not found");
                // Implement soft delete
                product.ApprovalStatus = ProductApprovalStatus.Deleted
;
                product.IsAvailable = false;
                product.UpdatedAt = DateTime.UtcNow;

                // Also mark all variants as unavailable
                foreach (var variant in product.ProductVariants)
                {
                    variant.IsAvailable = false;
                }

                // Remove from carts
                var cartItems = await _context.CartItems
                    .Where(ci => ci.ProductId == id)
                    .ToListAsync();
                _context.CartItems.RemoveRange(cartItems);

                // Remove from wishlists
                var wishlistItems = await _context.WishlistItems
                    .Where(wi => wi.ProductId == id)
                    .ToListAsync();
                _context.WishlistItems.RemoveRange(wishlistItems);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // Update a product's approval status (admin function)
        public async Task<ProductDto> UpdateProductApprovalStatusAsync(int id, string approvalStatus, string adminNotes = null)
        {
            var product = await _context.Products
                .Include(p => p.ProductVariants)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
                throw new KeyNotFoundException($"Product with ID {id} not found");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Update approval status
                product.ApprovalStatus = approvalStatus;

                // Update availability based on approval status
                product.IsAvailable = approvalStatus == ProductApprovalStatus.Approved;

                // Update variants availability based on product approval
                foreach (var variant in product.ProductVariants)
                {
                    variant.IsAvailable = approvalStatus == ProductApprovalStatus.Approved && variant.StockQuantity > 0;
                }

                // TODO: Create an AdminProductReview entity to store admin notes and review history

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetProductByIdAsync(id, true);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // Get products pending approval (admin function)
        public async Task<IEnumerable<ProductDto>> GetPendingApprovalProductsAsync(PaginationDto pagination)
        {
            var products = await _context.Products
                .Where(p => p.ApprovalStatus == ProductApprovalStatus.Pending || p.ApprovalStatus == ProductApprovalStatus.PendingReview)
                .OrderBy(p => p.CreatedAt)
                .Skip(pagination.PageSize * pagination.PageNumber)
                .Take(pagination.PageSize)
                .Include(p => p.Seller)
                .Include(p => p.Subcategory)
                .ThenInclude(sc => sc.Category)
                .ToListAsync();

            return products.Select(p => MapToProductDto(p, true));
        }

        // Get products for a specific seller
        public async Task<IEnumerable<ProductDto>> GetSellerProductsAsync(int sellerId, PaginationDto pagination, ProductFilterDto filter = null)
        {
            filter ??= new ProductFilterDto();

            var query = _context.Products
                .Where(p => p.SellerId == sellerId && p.ApprovalStatus !=                 ProductApprovalStatus.Deleted
);

            // Apply filters
            if (filter.SubcategoryId.HasValue)
            {
                query = query.Where(p => p.SubcategoryId == filter.SubcategoryId);
            }

            if (!string.IsNullOrEmpty(filter.ApprovalStatus))
            {
                query = query.Where(p => p.ApprovalStatus == filter.ApprovalStatus);
            }

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(p => p.Name.Contains(filter.SearchTerm));
            }

            // Apply sorting
            query = ApplySorting(query, filter.SortBy, filter.SortDirection);

            var products = await query
                .Skip(pagination.PageSize * pagination.PageNumber)
                .Take(pagination.PageSize)
                .Include(p => p.Subcategory)
                .ToListAsync();

            return products.Select(p => MapToProductDto(p, false));
        }

        // Add a product variant
        public async Task<ProductVariantDto> AddProductVariantAsync(int productId, CreateProductVariantDto variantDto)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
                throw new KeyNotFoundException($"Product with ID {productId} not found");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var variant = new ProductVariant
                {
                    ProductId = productId,
                    VariantName = variantDto.VariantName,
                    Price = variantDto.Price,
                    DiscountPercentage = variantDto.DiscountPercentage,
                    StockQuantity = variantDto.StockQuantity,
                    Sku = variantDto.Sku,
                    IsDefault = variantDto.IsDefault,
                    IsAvailable = product.ApprovalStatus == ProductApprovalStatus.Approved && variantDto.StockQuantity > 0
                };

                _context.ProductVariants.Add(variant);
                await _context.SaveChangesAsync();

                // Add variant attributes
                if (variantDto.Attributes != null && variantDto.Attributes.Any())
                {
                    foreach (var attr in variantDto.Attributes)
                    {
                        var variantAttribute = new VariantAttribute
                        {
                            VariantId = variant.VariantId,
                            AttributeName = attr.AttributeName,
                            AttributeValue = attr.AttributeValue
                        };

                        _context.VariantAttributes.Add(variantAttribute);
                    }

                    await _context.SaveChangesAsync();
                }

                // If this is set as default, update other variants
                if (variant.IsDefault == true)
                {
                    var otherVariants = await _context.ProductVariants
                        .Where(v => v.ProductId == productId && v.VariantId != variant.VariantId)
                        .ToListAsync();

                    foreach (var otherVariant in otherVariants)
                    {
                        otherVariant.IsDefault = false;
                    }

                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                return MapToProductVariantDto(variant);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // Update a product variant
        public async Task<ProductVariantDto> UpdateProductVariantAsync(int variantId, UpdateProductVariantDto variantDto)
        {
            var variant = await _context.ProductVariants
                .Include(v => v.VariantAttributes)
                .FirstOrDefaultAsync(v => v.VariantId == variantId);

            if (variant == null)
                throw new KeyNotFoundException($"Variant with ID {variantId} not found");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Update basic variant information
                variant.VariantName = variantDto.VariantName;
                variant.Price = variantDto.Price;
                variant.DiscountPercentage = variantDto.DiscountPercentage;
                variant.StockQuantity = variantDto.StockQuantity;
                variant.Sku = variantDto.Sku;
                variant.IsDefault = variantDto.IsDefault;

                // Check product approval status to determine availability
                var product = await _context.Products.FindAsync(variant.ProductId);
                variant.IsAvailable = product?.ApprovalStatus == ProductApprovalStatus.Approved && variantDto.StockQuantity > 0;

                await _context.SaveChangesAsync();

                // Update variant attributes
                if (variantDto.Attributes != null && variantDto.Attributes.Any())
                {
                    // Remove existing attributes
                    _context.VariantAttributes.RemoveRange(variant.VariantAttributes);
                    await _context.SaveChangesAsync();

                    // Add new attributes
                    foreach (var attr in variantDto.Attributes)
                    {
                        var variantAttribute = new VariantAttribute
                        {
                            VariantId = variant.VariantId,
                            AttributeName = attr.AttributeName,
                            AttributeValue = attr.AttributeValue
                        };

                        _context.VariantAttributes.Add(variantAttribute);
                    }

                    await _context.SaveChangesAsync();
                }

                // If this is set as default, update other variants
                if (variant.IsDefault == true)
                {
                    var otherVariants = await _context.ProductVariants
                        .Where(v => v.ProductId == variant.ProductId && v.VariantId != variant.VariantId)
                        .ToListAsync();

                    foreach (var otherVariant in otherVariants)
                    {
                        otherVariant.IsDefault = false;
                    }

                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                return MapToProductVariantDto(variant);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // Delete a product variant
        public async Task DeleteProductVariantAsync(int variantId)
        {
            var variant = await _context.ProductVariants
                .Include(v => v.VariantAttributes)
                .FirstOrDefaultAsync(v => v.VariantId == variantId);

            if (variant == null)
                throw new KeyNotFoundException($"Variant with ID {variantId} not found");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Check if this is the only variant
                var variantCount = await _context.ProductVariants
                    .CountAsync(v => v.ProductId == variant.ProductId);

                if (variantCount <= 1)
                {
                    throw new InvalidOperationException("Cannot delete the only variant of a product");
                }

                // If this is the default variant, set another one as default
                if (variant.IsDefault == true)
                {
                    var newDefaultVariant = await _context.ProductVariants
                        .FirstOrDefaultAsync(v => v.ProductId == variant.ProductId && v.VariantId != variantId);

                    if (newDefaultVariant != null)
                    {
                        newDefaultVariant.IsDefault = true;
                        await _context.SaveChangesAsync();
                    }
                }

                // Delete variant attributes
                _context.VariantAttributes.RemoveRange(variant.VariantAttributes);

                // Delete the variant
                _context.ProductVariants.Remove(variant);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // Update product main image
        public async Task<ProductDto> UpdateProductMainImageAsync(int id, string imagePath)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                throw new KeyNotFoundException($"Product with ID {id} not found");

            // Delete old image if exists
            if (!string.IsNullOrEmpty(product.MainImageUrl))
            {
                await _imageService.DeleteImageAsync(product.MainImageUrl);
            }

            // Update image path
            product.MainImageUrl = imagePath;
            await _context.SaveChangesAsync();

            return await GetProductByIdAsync(id, false);
        }

        // Add a product attribute value
        public async Task<ProductAttributeValueDto> AddProductAttributeValueAsync(int productId, CreateProductAttributeValueDto attributeValueDto)
        {
            var product = await _context.Products.FindAsync(productId);

            if (product == null)
                throw new KeyNotFoundException($"Product with ID {productId} not found");

            // Check if attribute exists
            var attribute = await _context.ProductAttributes.FindAsync(attributeValueDto.AttributeId);
            if (attribute == null)
                throw new KeyNotFoundException($"Attribute with ID {attributeValueDto.AttributeId} not found");

            // Check if attribute value already exists
            var existingValue = await _context.ProductAttributeValues
                .FirstOrDefaultAsync(av => av.ProductId == productId && av.AttributeId == attributeValueDto.AttributeId);

            if (existingValue != null)
                throw new InvalidOperationException($"Product already has a value for attribute {attribute.Name}");

            var attributeValue = new ProductAttributeValue
            {
                ProductId = productId,
                AttributeId = attributeValueDto.AttributeId,
                Value = attributeValueDto.Value
            };

            _context.ProductAttributeValues.Add(attributeValue);
            await _context.SaveChangesAsync();

            return new ProductAttributeValueDto
            {
                ValueId = attributeValue.ValueId,
                ProductId = attributeValue.ProductId,
                AttributeId = attributeValue.AttributeId,
                AttributeName = attribute.Name,
                Value = attributeValue.Value
            };
        }

        // Update a product attribute value
        public async Task UpdateProductAttributeValueAsync(int valueId, UpdateProductAttributeValueDto attributeValueDto)
        {
            var attributeValue = await _context.ProductAttributeValues.FindAsync(valueId);

            if (attributeValue == null)
                throw new KeyNotFoundException($"Attribute value with ID {valueId} not found");

            attributeValue.Value = attributeValueDto.Value;
            await _context.SaveChangesAsync();
        }

        // Update product stock
        public async Task<ProductDto> UpdateProductStockAsync(int id, int newStock)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                throw new KeyNotFoundException($"Product with ID {id} not found");

            product.StockQuantity = newStock;
            product.UpdatedAt = DateTime.UtcNow;

            // Update availability if approved
            if (product.ApprovalStatus == ProductApprovalStatus.Approved)
            {
                product.IsAvailable = newStock > 0;
            }

            await _context.SaveChangesAsync();

            return await GetProductByIdAsync(id, false);
        }

        // Update variant stock
        public async Task<ProductVariantDto> UpdateVariantStockAsync(int variantId, int newStock)
        {
            var variant = await _context.ProductVariants
                .Include(v => v.Product)
                .FirstOrDefaultAsync(v => v.VariantId == variantId);

            if (variant == null)
                throw new KeyNotFoundException($"Variant with ID {variantId} not found");

            variant.StockQuantity = newStock;

            // Update availability if product is approved
            if (variant.Product.ApprovalStatus == ProductApprovalStatus.Approved)
            {
                variant.IsAvailable = newStock > 0;
            }

            await _context.SaveChangesAsync();

            return MapToProductVariantDto(variant);
        }

        // Get product statistics
        public async Task<ProductStatisticsDto> GetProductStatisticsAsync(int productId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var product = await _context.Products
                .Include(p => p.Ratings)
                .Include(p => p.ProductViews)
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
                throw new KeyNotFoundException($"Product with ID {productId} not found");

            startDate ??= DateTime.UtcNow.AddMonths(-1);
            endDate ??= DateTime.UtcNow;

            // Filter views by date range
            var filteredViews = product.ProductViews
                .Where(v => v.ViewedAt >= startDate && v.ViewedAt <= endDate);

            // Get order items for the product in the date range
            var orderItems = await _context.OrderItems
                .Include(oi => oi.Suborder)
                .ThenInclude(so => so.Order)
                .Where(oi => oi.ProductId == productId &&
                       oi.Suborder.Order.CreatedAt >= startDate &&
                       oi.Suborder.Order.CreatedAt <= endDate)
                .ToListAsync();

            // Calculate statistics
            var totalViews = filteredViews.Count();
            var uniqueViews = filteredViews.Select(v =>
                v.CustomerId.HasValue && v.CustomerId > 0
                    ? v.CustomerId.ToString()
                    : v.SessionId
            ).Distinct().Count();
            var totalSales = orderItems.Sum(oi => oi.Quantity);
            var totalRevenue = orderItems.Sum(oi => oi.TotalPrice);
            var conversionRate = totalViews > 0 ? (double)totalSales / totalViews * 100 : 0;

            // Get average rating
            var avgRating = product.Ratings.Any()
                ? Math.Round(product.Ratings.Average(r => r.Stars), 1)
                : 0;

            return new ProductStatisticsDto
            {
                ProductId = productId,
                TotalViews = totalViews,
                UniqueViews = uniqueViews,
                TotalSales = totalSales,
                TotalRevenue = totalRevenue,
                ConversionRate = conversionRate,
                AverageRating = avgRating,
                StartDate = startDate.Value,
                EndDate = endDate.Value
            };
        }

        // Get related products
        public async Task<IEnumerable<ProductDto>> GetRelatedProductsAsync(int productId, int count = 5)
        {
            // First check if we have algorithmic recommendations for this product
            var recommendations = await _context.ProductRecommendations
                .Where(pr => pr.SourceProductId == productId)
                .OrderByDescending(pr => pr.Score)
                .Take(count)
                .Include(pr => pr.RecommendedProduct)
                .ThenInclude(p => p.Seller)
                .Include(pr => pr.RecommendedProduct.Subcategory)
                .ToListAsync();

            if (recommendations.Any())
            {
                return recommendations.Select(r => MapToProductDto(r.RecommendedProduct, false));
            }

            // Fallback: Get products from the same subcategory
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == productId &&
                   p.ApprovalStatus !=                 ProductApprovalStatus.Deleted
);

            if (product == null)
                throw new KeyNotFoundException($"Product with ID {productId} not found");

            var relatedProducts = await _context.Products
                .Where(p => p.SubcategoryId == product.SubcategoryId &&
                           p.ProductId != productId &&
                           p.ApprovalStatus == ProductApprovalStatus.Approved &&
                           p.IsAvailable == true &&
                           p.ApprovalStatus !=                 ProductApprovalStatus.Deleted
)
                .OrderByDescending(p => p.AverageRating)
                .Take(count)
                .Include(p => p.Seller)
                .Include(p => p.Subcategory)
                .ToListAsync();

            return relatedProducts.Select(p => MapToProductDto(p, false));
        }

        // Get trending products
        public async Task<IEnumerable<ProductDto>> GetTrendingProductsAsync(int categoryId = 0, int subcategoryId = 0, int count = 10)
        {
            // Start with a basic query without ordering
            var baseQuery = _context.TrendingProducts
                .Where(tp => tp.TimePeriod == "daily"); // Use daily trending data

            // Apply filters first
            if (categoryId > 0)
            {
                baseQuery = baseQuery.Where(tp => tp.CategoryId == categoryId);
            }

            if (subcategoryId > 0)
            {
                baseQuery = baseQuery.Where(tp => tp.SubcategoryId == subcategoryId);
            }

            // Apply the ordering after all filters
            var orderedQuery = baseQuery.OrderByDescending(tp => tp.TrendScore);

            var trending = await orderedQuery
                .Take(count)
                .Include(tp => tp.Product)
                .ThenInclude(p => p.Seller)
                .Include(tp => tp.Product.Subcategory)
                .ToListAsync();

            // Return only approved and available products
            return trending
                .Where(tp => tp.Product.ApprovalStatus == ProductApprovalStatus.Approved && tp.Product.IsAvailable == true &&
                    tp.Product.ApprovalStatus !=                 ProductApprovalStatus.Deleted
)
                .Select(tp => MapToProductDto(tp.Product, false));
        }
        #region Helper Methods

        // Map Product entity to ProductDto
        private ProductDto MapToProductDto(Product product, bool includeDetails)
        {
            if (product == null) return null;

            var productDto = new ProductDto
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                BasePrice = product.BasePrice,
                DiscountPercentage = product.DiscountPercentage ?? 0,
                FinalPrice = CalculateFinalPrice(product.BasePrice, product.DiscountPercentage),
                StockQuantity = product.StockQuantity,
                IsAvailable = product.IsAvailable ?? false,
                ApprovalStatus = product.ApprovalStatus,
                CreatedAt = product.CreatedAt ?? DateTime.MinValue,
                UpdatedAt = product.UpdatedAt ?? DateTime.MinValue,
                MainImageUrl = product.MainImageUrl,
                AverageRating = product.AverageRating ?? 0,
                SellerId = product.SellerId,
                SellerName = product.Seller?.BusinessName,
                SubcategoryId = product.SubcategoryId,
                SubcategoryName = product.Subcategory?.Name,
                CategoryId = product.Subcategory?.CategoryId ?? 0,
                CategoryName = product.Subcategory?.Category?.Name
            };

            if (includeDetails)
            {
                if (product.ProductImages != null)
                {
                    productDto.Images = product.ProductImages
                        .OrderBy(pi => pi.DisplayOrder)
                        .Select(pi => new ProductImageDto
                        {
                            ImageId = pi.ImageId,
                            ProductId = pi.ProductId,
                            ImageUrl = pi.ImageUrl,
                            DisplayOrder = pi.DisplayOrder ?? 0
                        })
                        .ToList();
                }

                if (product.ProductVariants != null)
                {
                    productDto.Variants = product.ProductVariants
                        .Select(pv => MapToProductVariantDto(pv))
                        .ToList();
                }

                if (product.ProductAttributeValues != null)
                {
                    productDto.AttributeValues = product.ProductAttributeValues
                        .Select(pav => new ProductAttributeValueDto
                        {
                            ValueId = pav.ValueId,
                            ProductId = pav.ProductId,
                            AttributeId = pav.AttributeId,
                            AttributeName = pav.Attribute?.Name,
                            Value = pav.Value,
                            AttributeType = pav.Attribute?.Type
                        })
                        .ToList();
                }

                if (product.Ratings != null)
                {
                    productDto.RatingCount = product.Ratings.Count;
                    productDto.ReviewCount = product.Ratings.Count(r => !string.IsNullOrEmpty(r.Comment));
                }
            }

            return productDto;
        }
        public async Task<IEnumerable<ProductDto>> GetRandomProductsByCategoryAsync(string categoryName, int count = 5)
        {
            var randomProducts = await _context.Products
                .Where(p => p.Subcategory.Category.Name == categoryName &&
                            p.ApprovalStatus == ProductApprovalStatus.Approved &&
                            p.IsAvailable == true)
                .OrderBy(x => EF.Functions.Random()) // Use database-specific random ordering
                .Take(count)
                .Include(p => p.Seller)
                .Include(p => p.Subcategory)
                .ToListAsync();

            return randomProducts.Select(p => MapToProductDto(p, false));
        }

        public async Task<IEnumerable<ProductDto>> GetRandomProductsBySubcategoryAsync(string subcategoryName, int count = 5)
        {
            var randomProducts = await _context.Products
                .Where(p => p.Subcategory.Name == subcategoryName &&
                            p.ApprovalStatus == ProductApprovalStatus.Approved &&
                            p.IsAvailable == true)
                .OrderBy(x => EF.Functions.Random()) 
                .Take(count)
                .Include(p => p.Seller)
                .Include(p => p.Subcategory)
                .ToListAsync();

            return randomProducts.Select(p => MapToProductDto(p, false));
        }
        // Map ProductVariant entity to ProductVariantDto
        private ProductVariantDto MapToProductVariantDto(ProductVariant variant)
        {
            if (variant == null) return null;

            return new ProductVariantDto
            {
                VariantId = variant.VariantId,
                ProductId = variant.ProductId,
                VariantName = variant.VariantName,
                Price = variant.Price,
                DiscountPercentage = variant.DiscountPercentage ?? 0,
                FinalPrice = CalculateFinalPrice(variant.Price, variant.DiscountPercentage),
                StockQuantity = variant.StockQuantity,
                Sku = variant.Sku,
                VariantImageUrl = variant.VariantImageUrl,
                IsDefault = variant.IsDefault ?? false,
                IsAvailable = variant.IsAvailable ?? false,
                Attributes = variant.VariantAttributes?.Select(va => new Jumia_Clone.Models.DTOs.ProductDTOs.VariantAttributeDto
                {
                    VariantAttributeId = va.VariantAttributeId,
                    VariantId = va.VariantId,
                    AttributeName = va.AttributeName,
                    AttributeValue = va.AttributeValue
                }).ToList()
            };
        }

        // Calculate final price after discount
        private decimal CalculateFinalPrice(decimal basePrice, decimal? discountPercentage)
        {
            if (!discountPercentage.HasValue || discountPercentage <= 0)
                return basePrice;

            var discountAmount = basePrice * (discountPercentage.Value / 100);
            return Math.Round(basePrice - discountAmount, 2);
        }

        // Apply sorting to product query
        private IQueryable<Product> ApplySorting(IQueryable<Product> query, string sortBy, string sortDirection)
        {
            sortBy = sortBy?.ToLower() ?? "createdat";
            sortDirection = sortDirection?.ToLower() ?? "desc";
            bool isAscending = sortDirection == "asc";

            switch (sortBy)
            {
                case "price":
                    return isAscending
                        ? query.OrderBy(p => p.BasePrice)
                        : query.OrderByDescending(p => p.BasePrice);
                case "name":
                    return isAscending
                        ? query.OrderBy(p => p.Name)
                        : query.OrderByDescending(p => p.Name);
                case "rating":
                    return isAscending
                        ? query.OrderBy(p => p.AverageRating)
                        : query.OrderByDescending(p => p.AverageRating);
                case "popularity":
                    // Popularity can be determined by number of views or sales
                    return query.OrderByDescending(p => p.ProductViews.Count);
                case "createdat":
                default:
                    return isAscending
                        ? query.OrderBy(p => p.CreatedAt)
                        : query.OrderByDescending(p => p.CreatedAt);
            }
        }

        #endregion
    }
}