using Jumia_Clone.Models.Constants;
using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Models.DTOs.ProductAttributeDTOs;
using Jumia_Clone.Models.DTOs.ProductAttributeValueDTOs;
using Jumia_Clone.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ProductAttributesController : ControllerBase
{
    private readonly IProductAttributeRepository _productAttributeRepository;
    private readonly ILogger<ProductAttributesController> _logger;

    public ProductAttributesController(
        IProductAttributeRepository productAttributeRepository,
        ILogger<ProductAttributesController> logger)
    {
        _productAttributeRepository = productAttributeRepository;
        _logger = logger;
    }

    // Create a new product attribute
    [HttpPost("attribute")]
    [Authorize(Roles = $"{UserRoles.Admin}")]
    public async Task<IActionResult> CreateProductAttribute([FromBody] CreateProductAttributeDto attributeDto)
    {
        try
        {
            var createdAttribute = await _productAttributeRepository.CreateProductAttributeAsync(attributeDto);

            return Ok(new ApiResponse<ProductAttributeDto>
            {
                Message = "Product attribute created successfully",
                Data = createdAttribute,
                Success = true
            });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Subcategory not found when creating product attribute");
            return NotFound(new ApiErrorResponse
            {
                Message = "Subcategory not found",
                ErrorMessages = new[] { ex.Message }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product attribute");
            return StatusCode(500, new ApiErrorResponse
            {
                Message = "Error creating product attribute",
                ErrorMessages = new[] { ex.Message }
            });
        }
    }

    // Update an existing product attribute
    [HttpPut("attribute/{attributeId}")]
    [Authorize(Roles = $"{UserRoles.Admin}")]
    public async Task<IActionResult> UpdateProductAttribute(
        int attributeId,
        [FromBody] UpdateProductAttributeDto attributeDto)
    {
        try
        {
            var updatedAttribute = await _productAttributeRepository.UpdateProductAttributeAsync(attributeId, attributeDto);

            return Ok(new ApiResponse<ProductAttributeDto>
            {
                Message = "Product attribute updated successfully",
                Data = updatedAttribute,
                Success = true
            });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, $"Product attribute with ID {attributeId} not found");
            return NotFound(new ApiErrorResponse
            {
                Message = "Product attribute not found",
                ErrorMessages = new[] { ex.Message }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating product attribute {attributeId}");
            return StatusCode(500, new ApiErrorResponse
            {
                Message = "Error updating product attribute",
                ErrorMessages = new[] { ex.Message }
            });
        }
    }

    // Delete a product attribute
    [HttpDelete("attribute/{attributeId}")]
    [Authorize(Roles = $"{UserRoles.Admin}")]
    public async Task<IActionResult> DeleteProductAttribute(int attributeId)
    {
        try
        {
            await _productAttributeRepository.DeleteProductAttributeAsync(attributeId);

            return Ok(new ApiResponse<object>
            {
                Message = "Product attribute deleted successfully",
                Success = true
            });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, $"Product attribute with ID {attributeId} not found");
            return NotFound(new ApiErrorResponse
            {
                Message = "Product attribute not found",
                ErrorMessages = new[] { ex.Message }
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, $"Cannot delete product attribute {attributeId}");
            return BadRequest(new ApiErrorResponse
            {
                Message = "Cannot delete attribute",
                ErrorMessages = new[] { ex.Message }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting product attribute {attributeId}");
            return StatusCode(500, new ApiErrorResponse
            {
                Message = "Error deleting product attribute",
                ErrorMessages = new[] { ex.Message }
            });
        }
    }

    // Get a specific product attribute by ID
    [HttpGet("attribute/{attributeId}")]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Seller}")]
    public async Task<IActionResult> GetProductAttributeById(int attributeId)
    {
        try
        {
            var attribute = await _productAttributeRepository.GetProductAttributeByIdAsync(attributeId);

            return Ok(new ApiResponse<ProductAttributeDto>
            {
                Message = "Product attribute retrieved successfully",
                Data = attribute,
                Success = true
            });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, $"Product attribute with ID {attributeId} not found");
            return NotFound(new ApiErrorResponse
            {
                Message = "Product attribute not found",
                ErrorMessages = new[] { ex.Message }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving product attribute {attributeId}");
            return StatusCode(500, new ApiErrorResponse
            {
                Message = "Error retrieving product attribute",
                ErrorMessages = new[] { ex.Message }
            });
        }
    }

    // Get all product attributes for all subcategories
    [HttpGet()]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Seller}")]
    public async Task<IActionResult> GetProductAttributes([FromQuery]PaginationDto pagination, bool include_details)
    {
        try
        {
            var attributes = await _productAttributeRepository.GetProductAttributes(pagination, include_details);
            var totalItems = await _productAttributeRepository.GetCountAsync();

            return Ok(new 
            {
                Message = "Product attributes retrieved successfully",
                Data = attributes,
                totalItems,
                Success = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving product attributes ");
            return StatusCode(500, new ApiErrorResponse
            {
                Message = "Error retrieving product attributes",
                ErrorMessages = new[] { ex.Message }
            });
        }
    }

    // Get all product attributes for a specific subcategory
    [HttpGet("subcategory/{subcategoryId}")]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Seller}")]
    public async Task<IActionResult> GetProductAttributesBySubcategory(int subcategoryId)
    {
        try
        {
            var attributes = await _productAttributeRepository.GetProductAttributesBySubcategoryAsync(subcategoryId);

            return Ok(new ApiResponse<IEnumerable<ProductAttributeDto>>
            {
                Message = "Product attributes retrieved successfully",
                Data = attributes,
                Success = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving product attributes for subcategory {subcategoryId}");
            return StatusCode(500, new ApiErrorResponse
            {
                Message = "Error retrieving product attributes",
                ErrorMessages = new[] { ex.Message }
            });
        }
    }

    // Product Attribute Value Methods

    // Add a product attribute value
    [HttpPost("value")]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Seller}")]

    public async Task<IActionResult> AddProductAttributeValue([FromBody] CreateProductAttributeValueDto attributeValueDto)
    {
        try
        {
            var addedAttributeValue = await _productAttributeRepository.AddProductAttributeValueAsync(attributeValueDto);

            return Ok(new ApiResponse<ProductAttributeValueDto>
            {
                Message = "Product attribute value added successfully",
                Data = addedAttributeValue,
                Success = true
            });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Product or attribute not found when adding attribute value");
            return NotFound(new ApiErrorResponse
            {
                Message = "Product or attribute not found",
                ErrorMessages = new[] { ex.Message }
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Attribute value already exists");
            return BadRequest(new ApiErrorResponse
            {
                Message = "Attribute value already exists",
                ErrorMessages = new[] { ex.Message }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding product attribute value");
            return StatusCode(500, new ApiErrorResponse
            {
                Message = "Error adding product attribute value",
                ErrorMessages = new[] { ex.Message }
            });
        }
    }

    // Update a product attribute value
    [HttpPut("value/{valueId}")]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Seller}")]

    public async Task<IActionResult> UpdateProductAttributeValue(
        int valueId,
        [FromBody] UpdateProductAttributeValueDto attributeValueDto)
    {
        try
        {
            var updatedAttributeValue = await _productAttributeRepository.UpdateProductAttributeValueAsync(valueId, attributeValueDto);

            return Ok(new ApiResponse<ProductAttributeValueDto>
            {
                Message = "Product attribute value updated successfully",
                Data = updatedAttributeValue,
                Success = true
            });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, $"Product attribute value with ID {valueId} not found");
            return NotFound(new ApiErrorResponse
            {
                Message = "Product attribute value not found",
                ErrorMessages = new[] { ex.Message }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating product attribute value {valueId}");
            return StatusCode(500, new ApiErrorResponse
            {
                Message = "Error updating product attribute value",
                ErrorMessages = new[] { ex.Message }
            });
        }
    }

    // Delete a product attribute value
    [HttpDelete("value/{valueId}")]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Seller}")]

    public async Task<IActionResult> DeleteProductAttributeValue(int valueId)
    {
        try
        {
            await _productAttributeRepository.DeleteProductAttributeValueAsync(valueId);

            return Ok(new ApiResponse<object>
            {
                Message = "Product attribute value deleted successfully",
                Success = true
            });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, $"Product attribute value with ID {valueId} not found");
            return NotFound(new ApiErrorResponse
            {
                Message = "Product attribute value not found",
                ErrorMessages = new[] { ex.Message }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting product attribute value {valueId}");
            return StatusCode(500, new ApiErrorResponse
            {
                Message = "Error deleting product attribute value",
                ErrorMessages = new[] { ex.Message }
            });
        }
    }

    // Get all attribute values for a specific product
    [HttpGet("product/{productId}/values")]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Seller}")]

    public async Task<IActionResult> GetProductAttributeValues(int productId)
    {
        try
        {
            var attributeValues = await _productAttributeRepository.GetProductAttributeValuesAsync(productId);

            return Ok(new ApiResponse<IEnumerable<ProductAttributeValueDto>>
            {
                Message = "Product attribute values retrieved successfully",
                Data = attributeValues,
                Success = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving product attribute values for product {productId}");
            return StatusCode(500, new ApiErrorResponse
            {
                Message = "Error retrieving product attribute values",
                ErrorMessages = new[] { ex.Message }
            });
        }
    }
}