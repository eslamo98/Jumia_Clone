using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Models.DTOs.ProductAttributeValueDTOs;
using Jumia_Clone.Models.DTOs.ProductDTOs;
using Jumia_Clone.Models.DTOs.ProductVariantDTOs;
using Jumia_Clone.Models.Entities;
using Jumia_Clone.Models.Enums;
using Jumia_Clone.Repositories.Interfaces;
using Jumia_Clone.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Jumia_Clone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductVariantsController : ControllerBase
    {
        private readonly IProductVariantsRepository _repository;
        private readonly IImageService _imageService;
        private readonly IProductRepository _productRepository;
        //private readonly IMemoryCache _cache;
        private readonly ILogger<ProductVariantsController> _logger;

        public ProductVariantsController(
            IProductVariantsRepository repository,
            IImageService imageService,
            IProductRepository productRepository,
            //IMemoryCache cache,
            ILogger<ProductVariantsController> logger)
        {
            _repository = repository;
            _imageService = imageService;
            _productRepository = productRepository;
            //_cache = cache;
            _logger = logger;
        }

        // Helper method to get current user ID
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("Unable to determine current user");
            }

            return userId;
        }

        // GET: api/ProductVariants
        [HttpGet]
        [EnableRateLimiting("standard")]
        [ResponseCache(Duration = 60)]
        public async Task<IActionResult> GetAll([FromQuery] int? productId = null)
        {
            try
            {
                var cacheKey = $"product_variants_{productId ?? 0}";

                // Try to get from cache first
                //if (_cache.TryGetValue(cacheKey, out ApiResponse<IEnumerable<ProductVariantDto>> cachedResult))
                //{
                //    return Ok(cachedResult);
                //}

                var variants = await _repository.GetAllAsync(productId);
                var variantDtos = new List<ProductVariantDto>();

                foreach (var variant in variants)
                {
                    var dto = MapToDto(variant);
                    variantDtos.Add(dto);
                }

                var response = new ApiResponse<IEnumerable<ProductVariantDto>>
                {
                    Success = true,
                    Message = "Variants retrieved successfully",
                    Data = variantDtos
                };

                // Cache the result
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(2));

                //_cache.Set(cacheKey, response, cacheEntryOptions);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving variants");
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving variants",
                    ErrorMessages = new[] { ex.Message }
                });
            }
        }

        // GET: api/ProductVariants/{id}
        [HttpGet("{id}")]
        [EnableRateLimiting("standard")]
        [ResponseCache(Duration = 60)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var cacheKey = $"product_variant_{id}";

                // Try to get from cache first
                //if (_cache.TryGetValue(cacheKey, out ApiResponse<ProductVariantDto> cachedResult))
                //{
                //    return Ok(cachedResult);
                //}

                var variant = await _repository.GetByIdWithDetailsAsync(id);
                if (variant == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Variant not found",
                        Data = null
                    });
                }

                var dto = MapToDto(variant);

                var response = new ApiResponse<ProductVariantDto>
                {
                    Success = true,
                    Message = "Variant retrieved successfully",
                    Data = dto
                };

                // Cache the result
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(2));

                //_cache.Set(cacheKey, response, cacheEntryOptions);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the variant: {Message}", ex.Message);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving the variant",
                    ErrorMessages = new[] { ex.Message }
                });
            }
        }

        // POST: api/ProductVariants
        [HttpPost]
        [Authorize(Roles = "seller, admin")]
        [EnableRateLimiting("strict")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] CreateProductVariantDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Invalid variant data",
                    ErrorMessages = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToArray()
                });
            }
            try
            {
                if (dto.VariantAttributesJson.StartsWith("'"))
                {
                    dto.VariantAttributesJson = dto.VariantAttributesJson.Substring(1);
                }

                if (dto.VariantAttributesJson.EndsWith("'"))
                {
                    dto.VariantAttributesJson = dto.VariantAttributesJson.Substring(0, dto.VariantAttributesJson.Length - 1);
                }
                dto.VariantAttributes = System.Text.Json.JsonSerializer.Deserialize<List<VariantAttributeInputDto>>(
                    dto.VariantAttributesJson);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Invalid variant attributes format",
                    ErrorMessages = new[] { ex.Message }
                });
            }
            try
            {
                // Verify product exists and belongs to seller (if seller role)
                var product = await _productRepository.GetProductByIdAsync(dto.ProductId);
                if (product == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Product not found",
                        Data = null
                    });
                }

                // Check if variant name exists for this product
                bool nameExists = await _repository.VariantNameExistsForProductAsync(dto.ProductId, dto.VariantName);
                if (nameExists)
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        Message = "Variant name already exists for this product",
                        ErrorMessages = new[] { "Please choose a different variant name" }
                    });
                }

                // Create variant entity from DTO
                var variantEntity = new ProductVariant
                {
                    ProductId = dto.ProductId,
                    VariantName = dto.VariantName,
                    Price = dto.Price,
                    DiscountPercentage = dto.DiscountPercentage,
                    StockQuantity = dto.StockQuantity,
                    Sku = dto.Sku,
                    IsDefault = dto.IsDefault ?? false,
                    IsAvailable = dto.IsAvailable ?? true
                };

                // Add the variant to database
                var createdVariant = await _repository.AddAsync(variantEntity);

                // Handle variant attributes if provided
                if (dto.VariantAttributes != null && dto.VariantAttributes.Any())
                {
                    foreach (var attrDto in dto.VariantAttributes)
                    {
                        var attribute = new VariantAttribute
                        {
                            VariantId = createdVariant.VariantId,
                            AttributeName = attrDto.Name,
                            AttributeValue = attrDto.Value
                        };
                        await _repository.AddVariantAttributeAsync(attribute);
                    }
                }

                // Handle image upload if provided
                if (dto.VariantImageFile != null && dto.VariantImageFile.Length > 0)
                {
                    string entityName = $"{product.Name}-variant-{createdVariant.VariantId}";

                    // Save image using ImageService
                    string imagePath = await _imageService.SaveImageAsync(
                        dto.VariantImageFile,
                        EntityType.ProductVariant,
                        entityName
                    );

                    // Update variant with image path
                    createdVariant = await _repository.UpdateVariantImageAsync(createdVariant.VariantId, imagePath);
                }

                // Get the complete variant with attributes for the response
                var completeVariant = await _repository.GetByIdWithDetailsAsync(createdVariant.VariantId);
                var resultDto = MapToDto(completeVariant);

                // Invalidate caches
                //InvalidateProductVariantCaches(dto.ProductId);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = resultDto.VariantId },
                    new ApiResponse<ProductVariantDto>
                    {
                        Success = true,
                        Message = "Variant created successfully",
                        Data = resultDto
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the variant: {Message}", ex.Message);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while creating the variant",
                    ErrorMessages = new[] { ex.Message }
                });
            }
        }

        // PUT: api/ProductVariants/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "seller, admin")]
        [EnableRateLimiting("strict")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateProductVariantDto dto)
        {
            if (id != dto.VariantId)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Invalid variant ID",
                    ErrorMessages = new[] { "ID in URL does not match ID in request body" }
                });
            }
            try
            {
                if (dto.VariantAttributesJson.StartsWith("'"))
                {
                    dto.VariantAttributesJson = dto.VariantAttributesJson.Substring(1);
                }

                if (dto.VariantAttributesJson.EndsWith("'"))
                {
                    dto.VariantAttributesJson = dto.VariantAttributesJson.Substring(0, dto.VariantAttributesJson.Length - 1);
                }
                dto.VariantAttributes = System.Text.Json.JsonSerializer.Deserialize<List<UpdateVariantAttributeDto>>(
                    dto.VariantAttributesJson);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Invalid variant attributes format",
                    ErrorMessages = new[] { ex.Message }
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Invalid variant data",
                    ErrorMessages = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToArray()
                });
            }

            try
            {
                // Check if variant exists
                var existingVariant = await _repository.GetByIdWithDetailsAsync(id);
                if (existingVariant == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Variant not found",
                        Data = null
                    });
                }

                // Verify product belongs to seller (if seller role)
                var product = await _productRepository.GetProductByIdAsync(existingVariant.ProductId);
                if (product == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Product not found",
                        Data = null
                    });
                }

                // Check if variant name exists (but exclude current variant)
                bool nameExists = await _repository.VariantNameExistsForProductAsync(
                    existingVariant.ProductId,
                    dto.VariantName,
                    id);

                if (nameExists)
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        Message = "Variant name already exists for this product",
                        ErrorMessages = new[] { "Please choose a different variant name" }
                    });
                }

                // Update variant properties
                existingVariant.VariantName = dto.VariantName;
                existingVariant.Price = dto.Price;
                existingVariant.DiscountPercentage = dto.DiscountPercentage;
                existingVariant.StockQuantity = dto.StockQuantity;
                existingVariant.Sku = dto.Sku;
                existingVariant.IsDefault = dto.IsDefault ?? existingVariant.IsDefault;
                existingVariant.IsAvailable = dto.IsAvailable ?? existingVariant.IsAvailable;

                // Update the variant in database
                var updatedVariant = await _repository.UpdateAsync(existingVariant, dto.VariantAttributes);



                // Handle image upload if provided
                if (dto.VariantImageFile != null && dto.VariantImageFile.Length > 0)
                {
                    string entityName = $"{product.Name}-variant-{id}";

                    // Update image using ImageService
                    string imagePath = await _imageService.UpdateImageAsync(
                        dto.VariantImageFile,
                        existingVariant.VariantImageUrl,
                        EntityType.ProductVariant,
                        entityName
                    );

                    // Update variant with new image path
                    updatedVariant = await _repository.UpdateVariantImageAsync(id, imagePath);
                }

                // Get the complete updated variant with attributes for the response
                var completeVariant = await _repository.GetByIdWithDetailsAsync(id);
                var resultDto = MapToDto(completeVariant);

                // Invalidate caches
                //InvalidateProductVariantCaches(existingVariant.ProductId);
                //_cache.Remove($"product_variant_{id}");

                return Ok(new ApiResponse<ProductVariantDto>
                {
                    Success = true,
                    Message = "Variant updated successfully",
                    Data = resultDto
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the variant: {Message}", ex.Message);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while updating the variant",
                    ErrorMessages = new[] { ex.Message }
                });
            }
        }

        // DELETE: api/ProductVariants/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "seller, admin")]
        [EnableRateLimiting("strict")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Get variant to check if it exists and get its image path
                var variant = await _repository.GetByIdWithDetailsAsync(id);
                if (variant == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Variant not found",
                        Data = null
                    });
                }

                int productId = variant.ProductId;

                // Verify product belongs to seller (if seller role)
                var product = await _productRepository.GetProductByIdAsync(variant.ProductId);
                if (product == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Product not found",
                        Data = null
                    });
                }

                // Store image path before deleting variant
                string imagePath = variant.VariantImageUrl;

                // Delete variant from database
                await _repository.DeleteAsync(id);

                // Delete the image file if it exists
                if (!string.IsNullOrEmpty(imagePath))
                {
                    await _imageService.DeleteImageAsync(imagePath);
                }

                // Invalidate caches
                //InvalidateProductVariantCaches(productId);
                //_cache.Remove($"product_variant_{id}");

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Variant deleted successfully",
                    Data = null
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Cannot delete variant",
                    ErrorMessages = new[] { ex.Message }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the variant: {Message}", ex.Message);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while deleting the variant",
                    ErrorMessages = new[] { ex.Message }
                });
            }
        }

        // POST: api/ProductVariants/{id}/set-default
        [HttpPost("{id}/set-default")]
        [Authorize(Roles = "seller, admin")]
        [EnableRateLimiting("strict")]
        public async Task<IActionResult> SetAsDefault(int id)
        {
            try
            {
                // Check if variant exists
                var variant = await _repository.GetByIdWithDetailsAsync(id);
                if (variant == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Variant not found",
                        Data = null
                    });
                }

                int productId = variant.ProductId;

                // Verify product belongs to seller (if seller role)
                var product = await _productRepository.GetProductByIdAsync(variant.ProductId);
                if (product == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Product not found",
                        Data = null
                    });
                }

                // Set as default
                var updatedVariant = await _repository.SetAsDefaultVariantAsync(id);

                // Get the complete updated variant with attributes for the response
                var completeVariant = await _repository.GetByIdWithDetailsAsync(id);
                var resultDto = MapToDto(completeVariant);

                // Invalidate caches
                //InvalidateProductVariantCaches(productId);

                return Ok(new ApiResponse<ProductVariantDto>
                {
                    Success = true,
                    Message = "Variant set as default successfully",
                    Data = resultDto
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while setting the default variant: {Message}", ex.Message);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while setting the default variant",
                    ErrorMessages = new[] { ex.Message }
                });
            }
        }

        // POST: api/ProductVariants/{id}/update-stock
        [HttpPost("{id}/update-stock")]
        [Authorize(Roles = "seller, admin")]
        [EnableRateLimiting("strict")]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] Jumia_Clone.Models.DTOs.ProductVariantDTOs.UpdateStockDto dto)
        {
            try
            {
                // Check if variant exists
                var variant = await _repository.GetByIdWithDetailsAsync(id);
                if (variant == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Variant not found",
                        Data = null
                    });
                }

                int productId = variant.ProductId;

                // Verify product belongs to seller (if seller role)
                var product = await _productRepository.GetProductByIdAsync(variant.ProductId);
                if (product == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Product not found",
                        Data = null
                    });
                }

                // Update stock
                var updatedVariant = await _repository.UpdateStockAsync(id, dto.StockQuantity);

                // Get the complete updated variant with attributes for the response
                var completeVariant = await _repository.GetByIdWithDetailsAsync(id);
                var resultDto = MapToDto(completeVariant);

                // Invalidate caches
                //InvalidateProductVariantCaches(productId);
                //_cache.Remove($"product_variant_{id}");

                return Ok(new ApiResponse<ProductVariantDto>
                {
                    Success = true,
                    Message = "Stock updated successfully",
                    Data = resultDto
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the stock: {Message}", ex.Message);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while updating the stock",
                    ErrorMessages = new[] { ex.Message }
                });
            }
        }

        // GET: api/ProductVariants/product/{productId}
        [HttpGet("product/{productId}")]
        [EnableRateLimiting("standard")]
        [ResponseCache(Duration = 60)]
        public async Task<IActionResult> GetByProductId(int productId)
        {
            try
            {
                var cacheKey = $"product_variants_by_product_{productId}";

                // Try to get from cache first
                //if (_cache.TryGetValue(cacheKey, out ApiResponse<IEnumerable<ProductVariantDto>> cachedResult))
                //{
                //    return Ok(cachedResult);
                //}

                var variants = await _repository.GetAllAsync(productId);
                var variantDtos = new List<ProductVariantDto>();

                foreach (var variant in variants)
                {
                    var dto = MapToDto(variant);
                    variantDtos.Add(dto);
                }

                var response = new ApiResponse<IEnumerable<ProductVariantDto>>
                {
                    Success = true,
                    Message = "Variants retrieved successfully",
                    Data = variantDtos
                };

                // Cache the result
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(2));

                //_cache.Set(cacheKey, response, cacheEntryOptions);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving variants for product {ProductId}: {Message}", productId, ex.Message);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving variants",
                    ErrorMessages = new[] { ex.Message }
                });
            }
        }

        // GET: api/ProductVariants/low-stock
        [HttpGet("low-stock")]
        [Authorize(Roles = "seller, admin")]
        [EnableRateLimiting("standard")]
        public async Task<IActionResult> GetLowStockVariants([FromQuery] int threshold = 10)
        {
            try
            {
                var cacheKey = $"product_variants_low_stock_{threshold}";

                // Try to get from cache first - short cache time for low stock items
                //if (_cache.TryGetValue(cacheKey, out ApiResponse<IEnumerable<ProductVariantDto>> cachedResult))
                //{
                //    return Ok(cachedResult);
                //}

                var variants = await _repository.GetLowStockVariantsAsync(threshold);
                var variantDtos = new List<ProductVariantDto>();

                foreach (var variant in variants)
                {
                    var dto = MapToDto(variant);
                    variantDtos.Add(dto);
                }

                var response = new ApiResponse<IEnumerable<ProductVariantDto>>
                {
                    Success = true,
                    Message = "Low stock variants retrieved successfully",
                    Data = variantDtos
                };

                // Cache the result for a shorter time since stock levels can change frequently
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(1));

                //_cache.Set(cacheKey, response, cacheEntryOptions);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving low stock variants: {Message}", ex.Message);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving low stock variants",
                    ErrorMessages = new[] { ex.Message }
                });
            }
        }

        // GET: api/ProductVariants/by-sku
        [HttpGet("by-sku")]
        [EnableRateLimiting("standard")]
        public async Task<IActionResult> GetBySku([FromQuery] string sku)
        {
            if (string.IsNullOrEmpty(sku))
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "SKU parameter is required",
                    ErrorMessages = new[] { "Please provide a SKU to search for" }
                });
            }

            try
            {
                var cacheKey = $"product_variants_by_sku_{sku}";

                // Try to get from cache first
                //if (_cache.TryGetValue(cacheKey, out ApiResponse<IEnumerable<ProductVariantDto>> cachedResult))
                //{
                //    return Ok(cachedResult);
                //}

                var variants = await _repository.GetVariantsBySKUAsync(sku);
                var variantDtos = new List<ProductVariantDto>();

                foreach (var variant in variants)
                {
                    var dto = MapToDto(variant);
                    variantDtos.Add(dto);
                }

                var response = new ApiResponse<IEnumerable<ProductVariantDto>>
                {
                    Success = true,
                    Message = "Variants retrieved successfully",
                    Data = variantDtos
                };

                // Cache the result
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(2));

                //_cache.Set(cacheKey, response, cacheEntryOptions);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving variants by SKU {SKU}: {Message}", sku, ex.Message);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving variants by SKU",
                    ErrorMessages = new[] { ex.Message }
                });
            }
        }

        // GET: api/ProductVariants/by-price-range
        [HttpGet("by-price-range")]
        [EnableRateLimiting("standard")]
        public async Task<IActionResult> GetByPriceRange([FromQuery] decimal minPrice = 0, [FromQuery] decimal maxPrice = 99999999.99m)
        {
            try
            {
                var cacheKey = $"product_variants_by_price_{minPrice}_{maxPrice}";

                // Try to get from cache first
                //if (_cache.TryGetValue(cacheKey, out ApiResponse<IEnumerable<ProductVariantDto>> cachedResult))
                //{
                //    return Ok(cachedResult);
                //}

                var variants = await _repository.GetVariantsByPriceRangeAsync(minPrice, maxPrice);
                var variantDtos = new List<ProductVariantDto>();

                foreach (var variant in variants)
                {
                    var dto = MapToDto(variant);
                    variantDtos.Add(dto);
                }

                var response = new ApiResponse<IEnumerable<ProductVariantDto>>
                {
                    Success = true,
                    Message = "Variants retrieved successfully",
                    Data = variantDtos
                };

                // Cache the result
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(2));

                //_cache.Set(cacheKey, response, cacheEntryOptions);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving variants by price range ({MinPrice} - {MaxPrice}): {Message}",
                    minPrice, maxPrice, ex.Message);
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving variants by price range",
                    ErrorMessages = new[] { ex.Message }
                });
            }
        }

        // Helper method to map a ProductVariant entity to a ProductVariantDto
        private ProductVariantDto MapToDto(ProductVariant variant)
        {
            if (variant == null)
                return null;

            var dto = new ProductVariantDto
            {
                VariantId = variant.VariantId,
                ProductId = variant.ProductId,
                VariantName = variant.VariantName,
                Price = variant.Price,
                DiscountPercentage = variant.DiscountPercentage,
                StockQuantity = variant.StockQuantity,
                Sku = variant.Sku,
                IsDefault = variant.IsDefault,
                IsAvailable = variant.IsAvailable
            };

            // Convert image URL if it exists
            if (!string.IsNullOrEmpty(variant.VariantImageUrl))
            {
                dto.VariantImageUrl = _imageService.GetImageUrl(variant.VariantImageUrl);
            }

            // Add variant attributes if they exist
            if (variant.VariantAttributes != null)
            {
                dto.VariantAttributes = variant.VariantAttributes.Select(attr => new UpdateVariantAttributeDto
                {
                    VariantId = attr.VariantAttributeId,
                    Name = attr.AttributeName,
                    Value = attr.AttributeValue
                }).ToList();
            }

            return dto;
        }

        // Helper method to invalidate product variant caches
        ////private void InvalidateProductVariantCaches(int productId)
        //{
        //    // Remove all caches related to variants
        ////    _cache.Remove($"product_variants_0"); // All variants
        ////    _cache.Remove($"product_variants_{productId}"); // Variants for specific product
        ////    _cache.Remove($"product_variants_by_product_{productId}"); // Variants by product endpoint

        //    // Low stock caches - multiple thresholds
        ////    _cache.Remove($"product_variants_low_stock_10"); // Default threshold
        ////    _cache.Remove($"product_variants_low_stock_5");
        ////    _cache.Remove($"product_variants_low_stock_20");

        //    // We don't know which price ranges or SKUs this variant might be part of,
        //    // so we rely on cache expiration for those

        //    // This will also trigger refreshes in related product caches
        //    string cacheKeyPattern = $"product_*_{productId}";

        //}
    }
}