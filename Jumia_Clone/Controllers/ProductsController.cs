using Jumia_Clone.Models.Constants;
using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Models.DTOs.ProductAttributeValueDTOs;
using Jumia_Clone.Models.DTOs.ProductDTOs;
using Jumia_Clone.Models.DTOs.ProductVariantDTOs2;
using Jumia_Clone.Models.Enums;
using Jumia_Clone.Repositories.Interfaces;
using Jumia_Clone.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Jumia_Clone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly IImageService _imageService;

        public ProductsController(IProductRepository productRepository, IImageService imageService)
        {
            _productRepository = productRepository;
            _imageService = imageService;
        }

        // GET: api/products
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationDto pagination, [FromQuery] ProductFilterDto filter)
        {
            try
            {
                // If not an admin, enforce approved status
                if (!User.IsInRole($"{UserRoles.Admin}"))
                {
                    // Ensure only approved products are retrieved for non-admin users
                    filter ??= new ProductFilterDto();
                    if (!string.IsNullOrEmpty(filter.ApprovalStatus) && IsValidApprovalStatus(filter.ApprovalStatus) && filter.ApprovalStatus != "approved")
                        return Unauthorized(new ApiErrorResponse()
                        {
                            Message = "You don't have the permission",
                            Success = false,
                            ErrorMessages = new string[0]
                        });
                }
                else
                {
                    // For admin, allow filtering by any valid status
                    if (!string.IsNullOrEmpty(filter?.ApprovalStatus) && !IsValidApprovalStatus(filter.ApprovalStatus))
                    {
                        return BadRequest(new ApiErrorResponse
                        {
                            Message = "Invalid approval status",
                            ErrorMessages = new[] { "The provided approval status is not valid." }
                        });
                    }
                }

                var products = await _productRepository.GetAllProductsAsync(pagination, filter);
                var totalItems = await _productRepository.GetProductsCount();
                // Convert image paths to URLs
                foreach (var product in products)
                {
                    if (!string.IsNullOrEmpty(product.MainImageUrl))
                    {
                        product.MainImageUrl = _imageService.GetImageUrl(product.MainImageUrl);
                    }

                    // Optionally, handle variant images
                    if (product.Variants != null)
                    {
                        foreach (var variant in product.Variants)
                        {
                            if (!string.IsNullOrEmpty(variant.VariantImageUrl))
                            {
                                variant.VariantImageUrl = _imageService.GetImageUrl(variant.VariantImageUrl);
                            }
                        }
                    }
                }

                return Ok(new
                {
                    Message = "Successfully retrieved products",
                    Data = new
                    {
                        products = products,
                        totalItems = totalItems,
                    },
                    Success = true
                });
            }
            catch (Exception ex)
            {
                // Log the exception

                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving products",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // GET: api/products/random/category
        [HttpGet("random/category")]
        public async Task<IActionResult> GetRandomProductsByCategory(
            [FromQuery] string categoryName,
            [FromQuery] int count = 5)
        {
            try
            {
                var products = await _productRepository.GetRandomProductsByCategoryAsync(categoryName, count);

                // Convert image paths to URLs
                foreach (var product in products)
                {
                    if (!string.IsNullOrEmpty(product.MainImageUrl))
                    {
                        product.MainImageUrl = _imageService.GetImageUrl(product.MainImageUrl);
                    }
                }

                return Ok(new ApiResponse<IEnumerable<ProductDto>>
                {
                    Message = $"Successfully retrieved {count} random products from category {categoryName}",
                    Data = products,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving random products",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // GET: api/products/random/subcategory
        [HttpGet("random/subcategory")]
        public async Task<IActionResult> GetRandomProductsBySubcategory(
            [FromQuery] string subcategoryName,
            [FromQuery] int count = 5)
        {
            try
            {
                var products = await _productRepository.GetRandomProductsBySubcategoryAsync(subcategoryName, count);

                // Convert image paths to URLs
                foreach (var product in products)
                {
                    if (!string.IsNullOrEmpty(product.MainImageUrl))
                    {
                        product.MainImageUrl = _imageService.GetImageUrl(product.MainImageUrl);
                    }
                }

                return Ok(new ApiResponse<IEnumerable<ProductDto>>
                {
                    Message = $"Successfully retrieved {count} random products from subcategory {subcategoryName}",
                    Data = products,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving random products",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }
        // GET: api/products/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id, [FromQuery] bool includeDetails = false)
        {
            try
            {
                var product = await _productRepository.GetProductByIdAsync(id, includeDetails);

                if (product == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Product not found",
                        Success = false,
                        Data = null
                    });
                }

                // Convert image paths to URLs
                if (!string.IsNullOrEmpty(product.MainImageUrl))
                {
                    product.MainImageUrl = _imageService.GetImageUrl(product.MainImageUrl);
                }

                if (includeDetails && product.Images != null)
                {
                    foreach (var image in product.Images)
                    {
                        image.ImageUrl = _imageService.GetImageUrl(image.ImageUrl);
                    }
                }

                if (includeDetails && product.Variants != null)
                {
                    foreach (var variant in product.Variants)
                    {
                        if (!string.IsNullOrEmpty(variant.VariantImageUrl))
                        {
                            variant.VariantImageUrl = _imageService.GetImageUrl(variant.VariantImageUrl);
                        }
                    }
                }

                return Ok(new ApiResponse<ProductDto>
                {
                    Message = "Product retrieved successfully",
                    Data = product,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving the product",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }
        [HttpPost()]
[Consumes("multipart/form-data")]
public async Task<IActionResult> Create([FromForm] CreateProductInputDto productDto)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(new ApiErrorResponse
        {
            Message = "Invalid product data",
            ErrorMessages = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToArray()
        });
    }
    try
    {
        // Process attribute values JSON
        if (!string.IsNullOrEmpty(productDto.ProductAttributeValuesJson))
        {
            try
            {
                if (productDto.ProductAttributeValuesJson.StartsWith("'"))
                {
                    productDto.ProductAttributeValuesJson = productDto.ProductAttributeValuesJson.Substring(1);
                }
                if (productDto.ProductAttributeValuesJson.EndsWith("'"))
                {
                    productDto.ProductAttributeValuesJson = productDto.ProductAttributeValuesJson.Substring(0, productDto.ProductAttributeValuesJson.Length - 1);
                }
                productDto.AttributeValues = System.Text.Json.JsonSerializer.Deserialize<List<CreateProductAttributeValueDto>>(productDto.ProductAttributeValuesJson);
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Process product variants JSON
        if (productDto.HasVariants && !string.IsNullOrEmpty(productDto.ProductVariantsJson))
        {
            try
            {
                if (productDto.ProductVariantsJson.StartsWith("'"))
                {
                    productDto.ProductVariantsJson = productDto.ProductVariantsJson.Substring(1);
                }
                if (productDto.ProductVariantsJson.EndsWith("'"))
                {
                    productDto.ProductVariantsJson = productDto.ProductVariantsJson.Substring(0, productDto.ProductVariantsJson.Length - 1);
                }
                productDto.Variants = System.Text.Json.JsonSerializer.Deserialize<List<CreateProductBaseVariantDto>>(productDto.ProductVariantsJson);

                // Validate at least one variant is marked as default
                if (!productDto.Variants.Any(v => v.IsDefault))
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        Message = "At least one variant must be marked as default",
                        ErrorMessages = new string[] { "No default variant specified" }
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Invalid product variants data",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // Check if user is admin for auto-approval
        bool isAdmin = User.IsInRole("Admin");

        // Create the product
        var createdProduct = await _productRepository.CreateProductAsync(productDto, isAdmin);

        // Handle main image upload if provided
        if (productDto.MainImageFile != null && productDto.MainImageFile.Length > 0)
        {
            string entityName = $"{createdProduct.Name}-{createdProduct.ProductId}";
            string imagePath = await _imageService.SaveImageAsync(
                productDto.MainImageFile,
                EntityType.Product,
                entityName
            );
            // Update the product with the image path
            createdProduct = await _productRepository.UpdateProductMainImageAsync(
                createdProduct.ProductId,
                imagePath
            );
            // Add the image URL to the response
            createdProduct.MainImageUrl = _imageService.GetImageUrl(imagePath);
        }

        // Handle additional product images if provided
        if (productDto.AdditionalImageFiles != null && productDto.AdditionalImageFiles.Any())
        {
            // Get the product image repository from DI
            var productImageRepository = HttpContext.RequestServices.GetRequiredService<IProductImageRepository>();
            
            var createProductImageDto = new Models.DTOs.ProductImageDTOs.CreateProductImageDto
            {
                ProductId = createdProduct.ProductId,
                ImageFiles = productDto.AdditionalImageFiles
            };
            
            // Save the additional images
            await productImageRepository.AddProductImagesAsync(createProductImageDto);
            
            // Refresh the product to include the newly added images
            createdProduct = await _productRepository.GetProductByIdAsync(createdProduct.ProductId, true);
            
            // Convert image URLs for the response
            if (createdProduct.Images != null)
            {
                foreach (var image in createdProduct.Images)
                {
                    image.ImageUrl = _imageService.GetImageUrl(image.ImageUrl);
                }
            }
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = createdProduct.ProductId },
            new ApiResponse<ProductDto>
            {
                Data = createdProduct,
                Message = isAdmin ? "Product created and approved successfully" : "Product created successfully (pending approval)",
                Success = true
            }
        );
    }
    catch (DbUpdateException ex)
    {
        return StatusCode(500, new ApiErrorResponse
        {
            Message = "An error occurred in the database while creating the product",
            ErrorMessages = new string[] { ex.Message }
        });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new ApiErrorResponse
        {
            Message = "An error occurred while creating the product",
            ErrorMessages = new string[] { ex.Message }
        });
    }
}
        // PUT: api/products/{id}
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateProductInputDto productDto)
        {
            if (id != productDto.ProductId)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Invalid product ID",
                    ErrorMessages = new string[] { "ID in URL does not match ID in request body" }
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Invalid product data",
                    ErrorMessages = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToArray()
                });
            }

            try
            {
                // Get existing product to check if it exists
                var existingProduct = await _productRepository.GetProductByIdAsync(id);
                if (existingProduct == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Product not found",
                        Success = false,
                        Data = null
                    });
                }



                // Update basic product info
                var updatedProduct = await _productRepository.UpdateProductAsync(id, productDto);

                // Handle main image update if provided
                if (productDto.MainImageFile != null && productDto.MainImageFile.Length > 0)
                {
                    string entityName = $"{updatedProduct.Name}-{id}";
                    string imagePath = await _imageService.UpdateImageAsync(
                        productDto.MainImageFile,
                        existingProduct.MainImageUrl,
                        EntityType.Product,
                        entityName
                    );

                    // Update the product with the new image path
                    updatedProduct = await _productRepository.UpdateProductMainImageAsync(id, imagePath);

                    // Add the image URL to the response
                    updatedProduct.MainImageUrl = _imageService.GetImageUrl(imagePath);
                }

                // Check if product status changed to pending review
                string statusMessage = updatedProduct.ApprovalStatus == "pending_review"
                    ? "Product updated successfully (pending review)"
                    : "Product updated successfully";

                return Ok(new ApiResponse<ProductDto>
                {
                    Data = updatedProduct,
                    Message = statusMessage,
                    Success = true
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>
                {
                    Message = ex.Message,
                    Success = false,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while updating the product",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // DELETE: api/products/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Check if product exists before deletion
                var product = await _productRepository.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Product not found",
                        Success = false,
                        Data = null
                    });
                }

                // Delete the product and all related data
                await _productRepository.DeleteProductAsync(id);

                return Ok(new ApiResponse<object>
                {
                    Message = "Product deleted successfully",
                    Data = null,
                    Success = true
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>
                {
                    Message = ex.Message,
                    Success = false,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while deleting the product",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // PUT: api/products/{id}/approval-status
        [HttpPut("{id}/approval-status")]
        public async Task<IActionResult> UpdateApprovalStatus(int id, [FromBody] UpdateApprovalStatusDto statusDto)
        {
            try
            {
                // Validate status values
                if (!IsValidApprovalStatus(statusDto.ApprovalStatus))
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        Message = "Invalid approval status",
                        ErrorMessages = new string[] { "Valid statuses are: pending, approved, rejected, pending_review" }
                    });
                }

                var product = await _productRepository.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Product not found",
                        Success = false,
                        Data = null
                    });
                }

                // Update the approval status
                var updatedProduct = await _productRepository.UpdateProductApprovalStatusAsync(
                    id,
                    statusDto.ApprovalStatus,
                    statusDto.AdminNotes
                );

                return Ok(new ApiResponse<ProductDto>
                {
                    Data = updatedProduct,
                    Message = $"Product approval status updated to {statusDto.ApprovalStatus}",
                    Success = true
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>
                {
                    Message = ex.Message,
                    Success = false,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while updating the product approval status",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // GET: api/products/pending-approval
        [HttpGet("pending-approval")]
        public async Task<IActionResult> GetPendingApproval([FromQuery] PaginationDto pagination)
        {
            try
            {
                var products = await _productRepository.GetPendingApprovalProductsAsync(pagination);

                // Convert image paths to URLs
                foreach (var product in products)
                {
                    if (!string.IsNullOrEmpty(product.MainImageUrl))
                    {
                        product.MainImageUrl = _imageService.GetImageUrl(product.MainImageUrl);
                    }
                }

                return Ok(new ApiResponse<IEnumerable<ProductDto>>
                {
                    Message = "Successfully retrieved pending approval products",
                    Data = products,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving pending approval products",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // GET: api/products/seller/{sellerId}
        [HttpGet("seller/{sellerId}")]
        public async Task<IActionResult> GetSellerProducts(int sellerId, [FromQuery] PaginationDto pagination, [FromQuery] ProductFilterDto filter)
        {
            try
            {
                var products = await _productRepository.GetSellerProductsAsync(sellerId, pagination, filter);

                // Convert image paths to URLs
                foreach (var product in products)
                {
                    if (!string.IsNullOrEmpty(product.MainImageUrl))
                    {
                        product.MainImageUrl = _imageService.GetImageUrl(product.MainImageUrl);
                    }
                }

                return Ok(new ApiResponse<IEnumerable<ProductDto>>
                {
                    Message = "Successfully retrieved seller products",
                    Data = products,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving seller products",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // GET: api/products/{id}/statistics
        [HttpGet("{id}/statistics")]
        public async Task<IActionResult> GetProductStatistics(int id, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var statistics = await _productRepository.GetProductStatisticsAsync(id, startDate, endDate);

                return Ok(new ApiResponse<ProductStatisticsDto>
                {
                    Message = "Successfully retrieved product statistics",
                    Data = statistics,
                    Success = true
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>
                {
                    Message = ex.Message,
                    Success = false,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving product statistics",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // GET: api/products/{id}/related
        [HttpGet("{id}/related")]
        public async Task<IActionResult> GetRelatedProducts(int id, [FromQuery] int count = 5)
        {
            try
            {
                var products = await _productRepository.GetRelatedProductsAsync(id, count);

                // Convert image paths to URLs
                foreach (var product in products)
                {
                    if (!string.IsNullOrEmpty(product.MainImageUrl))
                    {
                        product.MainImageUrl = _imageService.GetImageUrl(product.MainImageUrl);
                    }
                }

                return Ok(new ApiResponse<IEnumerable<ProductDto>>
                {
                    Message = "Successfully retrieved related products",
                    Data = products,
                    Success = true
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>
                {
                    Message = ex.Message,
                    Success = false,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving related products",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // GET: api/products/trending
        [HttpGet("trending")]
        public async Task<IActionResult> GetTrendingProducts([FromQuery] int categoryId = 0, [FromQuery] int subcategoryId = 0, [FromQuery] int count = 10)
        {
            try
            {
                var products = await _productRepository.GetTrendingProductsAsync(categoryId, subcategoryId, count);

                // Convert image paths to URLs
                foreach (var product in products)
                {
                    if (!string.IsNullOrEmpty(product.MainImageUrl))
                    {
                        product.MainImageUrl = _imageService.GetImageUrl(product.MainImageUrl);
                    }
                }

                return Ok(new ApiResponse<IEnumerable<ProductDto>>
                {
                    Message = "Successfully retrieved trending products",
                    Data = products,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving trending products",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // POST: api/products/{id}/variants
        [HttpPost("{id}/variants")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddVariant(int id, [FromForm] CreateProductVariantDto variantDto)
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
                // Check if product exists
                var product = await _productRepository.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Product not found",
                        Success = false,
                        Data = null
                    });
                }

                // Add the variant
                var createdVariant = await _productRepository.AddProductVariantAsync(id, variantDto);

                // Handle variant image upload if provided
                if (variantDto.VariantImageFile != null && variantDto.VariantImageFile.Length > 0)
                {
                    string entityName = $"{product.Name}-variant-{createdVariant.VariantId}";
                    string imagePath = await _imageService.SaveImageAsync(
                        variantDto.VariantImageFile,
                        EntityType.ProductVariant,
                        entityName
                    );

                    // Update the variant with the image path
                    // Note: We would need to add this method to the repository
                    // updatedVariant = await _productRepository.UpdateVariantImageAsync(createdVariant.VariantId, imagePath);

                    // For now, we'll assume the variant was updated with the image path
                    createdVariant.VariantImageUrl = _imageService.GetImageUrl(imagePath);
                }

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = id, includeDetails = true },
                    new ApiResponse<ProductVariantDto>
                    {
                        Data = createdVariant,
                        Message = "Variant added successfully",
                        Success = true
                    }
                );
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>
                {
                    Message = ex.Message,
                    Success = false,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while adding the variant",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // PUT: api/products/variants/{variantId}
        [HttpPut("variants/{variantId}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateVariant(int variantId, [FromForm] UpdateProductVariantDto variantDto)
        {
            if (variantId != variantDto.VariantId)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Invalid variant ID",
                    ErrorMessages = new string[] { "ID in URL does not match ID in request body" }
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
                var updatedVariant = await _productRepository.UpdateProductVariantAsync(variantId, variantDto);

                return Ok(new ApiResponse<ProductVariantDto>
                {
                    Data = updatedVariant,
                    Message = "Variant updated successfully",
                    Success = true
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>
                {
                    Message = ex.Message,
                    Success = false,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while updating the variant",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // DELETE: api/products/variants/{variantId}
        [HttpDelete("variants/{variantId}")]
        public async Task<IActionResult> DeleteVariant(int variantId)
        {
            try
            {
                await _productRepository.DeleteProductVariantAsync(variantId);

                return Ok(new ApiResponse<object>
                {
                    Message = "Variant deleted successfully",
                    Data = null,
                    Success = true
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>
                {
                    Message = ex.Message,
                    Success = false,
                    Data = null
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Cannot delete variant",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while deleting the variant",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // POST: api/products/{id}/stock
        [HttpPost("{id}/stock")]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] UpdatedStockDto stockDto)
        {
            try
            {
                var updatedProduct = await _productRepository.UpdateProductStockAsync(id, stockDto.NewStock);

                return Ok(new ApiResponse<ProductDto>
                {
                    Data = updatedProduct,
                    Message = "Stock updated successfully",
                    Success = true
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>
                {
                    Message = ex.Message,
                    Success = false,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while updating the stock",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // POST: api/products/variants/{variantId}/stock
        [HttpPost("variants/{variantId}/stock")]
        public async Task<IActionResult> UpdateVariantStock(int variantId, [FromBody] UpdatedStockDto stockDto)
        {
            try
            {
                var updatedVariant = await _productRepository.UpdateVariantStockAsync(variantId, stockDto.NewStock);

                return Ok(new ApiResponse<ProductVariantDto>
                {
                    Data = updatedVariant,
                    Message = "Variant stock updated successfully",
                    Success = true
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>
                {
                    Message = ex.Message,
                    Success = false,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while updating the variant stock",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // Helper to validate approval status
        private bool IsValidApprovalStatus(string status)
        {
            return new[] { "pending", "approved", "rejected", "pending_review", "deleted" }.Contains(status);
        }

    }

    // Additional DTOs needed for the controller
    public class UpdateApprovalStatusDto
    {
        public string ApprovalStatus { get; set; }
        public string AdminNotes { get; set; }
    }

    public class UpdatedStockDto
    {
        public int NewStock { get; set; }
    }

}