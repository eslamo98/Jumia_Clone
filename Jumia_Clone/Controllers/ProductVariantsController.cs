using System.Text.Json;
using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Models.DTOs.ProductVariantDTOs;
using Jumia_Clone.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Jumia_Clone.Controllers
{
    [Route("api/seller/products")]
    [ApiController]
   public class ProductVariantsController : ControllerBase
    {
        private readonly IProductVariantRepository _variantRepository;

        public ProductVariantsController(IProductVariantRepository variantRepository)
        {
            _variantRepository = variantRepository;
        }

        // GET: api/seller/products/{productId}/variants
        [HttpGet("{productId}/variants")]
        public async Task<IActionResult> GetAllVariants(int productId, [FromQuery] PaginationDto pagination)
        {
            try
            {
                var variants = await _variantRepository.GetAllVariantsByProductIdAsync(productId, pagination);
                return Ok(new ApiResponse<IEnumerable<ProductVariantDto>>
                {
                    Message = "Successfully retrieved all variants",
                    Data = variants,
                    Success = true
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse
                {
                    Message = ex.Message,
                    ErrorMessages = new string[] { ex.Message }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving variants",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // GET: api/seller/products/{productId}/variants/{variantId}
        [HttpGet("{productId}/variants/{variantId}")]
        public async Task<IActionResult> GetVariantById(int productId, int variantId)
        {
            try
            {
                var variant = await _variantRepository.GetVariantByIdAsync(productId, variantId);
                return Ok(new ApiResponse<ProductVariantDto>
                {
                    Message = "Variant retrieved successfully",
                    Data = variant,
                    Success = true
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse
                {
                    Message = ex.Message,
                    ErrorMessages = new string[] { ex.Message }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving the variant",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // POST: api/seller/products/{productId}/variants
        [HttpPost("{productId}/variants")]
        public async Task<IActionResult> AddOrUpdateVariant(int productId, [FromBody] JsonElement variantRequest)
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
                // Check if variant_id is provided for update
                if (variantRequest.TryGetProperty("variant_id", out JsonElement variantIdProperty) &&
                    variantIdProperty.ValueKind != JsonValueKind.Null)
                {
                    // Update existing variant
                    var updateDto = new UpdateProductVariantDto
                    {
                        VariantId = variantIdProperty.GetInt32(),
                        VariantName = GetStringProperty(variantRequest, "variant_name"),
                        Price = GetDecimalProperty(variantRequest, "price"),
                        DiscountPercentage = GetDecimalProperty(variantRequest, "discount_percentage"),
                        StockQuantity = GetInt32Property(variantRequest, "stock_quantity"),
                        SKU = GetStringProperty(variantRequest, "sku"),
                        VariantImageUrl = GetStringProperty(variantRequest, "variant_image_url"),
                        IsDefault = GetBooleanProperty(variantRequest, "is_default"),
                        Attributes = ExtractAttributes(variantRequest)
                    };

                    var updatedVariant = await _variantRepository.UpdateVariantAsync(productId, updateDto);
                    return Ok(new ApiResponse<object>
                    {
                        Message = "Variant updated successfully",
                        Data = new
                        {
                            variant_id = updatedVariant.VariantId,
                            variant_name = updatedVariant.VariantName,
                            price = updatedVariant.Price
                        },
                        Success = true
                    });
                }
                else
                {
                    // Create new variant
                    var createDto = new CreateProductVariantDto
                    {
                        VariantName = GetStringProperty(variantRequest, "variant_name"),
                        Price = GetDecimalProperty(variantRequest, "price"),
                        DiscountPercentage = GetDecimalProperty(variantRequest, "discount_percentage"),
                        StockQuantity = GetInt32Property(variantRequest, "stock_quantity"),
                        SKU = GetStringProperty(variantRequest, "sku"),
                        VariantImageUrl = GetStringProperty(variantRequest, "variant_image_url"),
                        IsDefault = GetBooleanProperty(variantRequest, "is_default"),
                        Attributes = ExtractAttributes(variantRequest)
                    };

                    var createdVariant = await _variantRepository.CreateVariantAsync(productId, createDto);
                    return CreatedAtAction(
                        nameof(GetVariantById),
                        new { productId = productId, variantId = createdVariant.VariantId },
                        new ApiResponse<object>
                        {
                            Message = "Variant added successfully",
                            Data = new
                            {
                                variant_id = createdVariant.VariantId,
                                variant_name = createdVariant.VariantName,
                                price = createdVariant.Price
                            },
                            Success = true
                        }
                    );
                }
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse
                {
                    Message = ex.Message,
                    ErrorMessages = new string[] { ex.Message }
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "Database error occurred",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while processing the variant",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // DELETE: api/seller/products/{productId}/variants/{variantId}
        [HttpDelete("{productId}/variants/{variantId}")]
        public async Task<IActionResult> DeleteVariant(int productId, int variantId)
        {
            try
            {
                await _variantRepository.DeleteVariantAsync(productId, variantId);
                return Ok(new ApiResponse<object>
                {
                    Message = "Variant deleted successfully",
                    Data = null,
                    Success = true
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse
                {
                    Message = ex.Message,
                    ErrorMessages = new string[] { ex.Message }
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = ex.Message,
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

        // Helper methods for safely extracting properties from JsonElement
        private string GetStringProperty(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out JsonElement property) &&
                property.ValueKind != JsonValueKind.Null)
            {
                return property.GetString();
            }
            return null;
        }

        private decimal GetDecimalProperty(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out JsonElement property) &&
                property.ValueKind != JsonValueKind.Null)
            {
                return property.GetDecimal();
            }
            return 0;
        }

        private int GetInt32Property(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out JsonElement property) &&
                property.ValueKind != JsonValueKind.Null)
            {
                return property.GetInt32();
            }
            return 0;
        }

        private bool GetBooleanProperty(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out JsonElement property) &&
                property.ValueKind != JsonValueKind.Null)
            {
                return property.GetBoolean();
            }
            return false;
        }

        private List<VariantAttributeDto> ExtractAttributes(JsonElement element)
        {
            var attributes = new List<VariantAttributeDto>();

            if (element.TryGetProperty("attributes", out JsonElement attributesArray) &&
                attributesArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var attr in attributesArray.EnumerateArray())
                {
                    string attributeName = null;
                    string attributeValue = null;

                    if (attr.TryGetProperty("attribute_name", out JsonElement nameElement))
                    {
                        attributeName = nameElement.GetString();
                    }

                    if (attr.TryGetProperty("attribute_value", out JsonElement valueElement))
                    {
                        attributeValue = valueElement.GetString();
                    }

                    if (attributeName != null && attributeValue != null)
                    {
                        attributes.Add(new VariantAttributeDto
                        {
                            AttributeName = attributeName,
                            AttributeValue = attributeValue
                        });
                    }
                }
            }

            return attributes;
        }
    }

    //[Route("api/admin/products")]
    //[ApiController]
    //[Authorize(Roles = "Admin")] // Assuming you have role-based authorization
    //public class AdminProductsController : ControllerBase
    //{
    //    private readonly IProductVariantRepository _variantRepository;

    //    public AdminProductsController(IProductVariantRepository variantRepository)
    //    {
    //        _variantRepository = variantRepository;
    //    }

    //    // PUT: api/admin/products/{id}/approval
    //    [HttpPut("{id}/approval")]
        //public async Task<IActionResult> UpdateProductApproval(int id, [FromBody] ProductApprovalDto approvalDto)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(new ApiErrorResponse
        //        {
        //            Message = "Invalid approval data",
        //            ErrorMessages = ModelState.Values
        //                .SelectMany(v => v.Errors)
        //                .Select(e => e.ErrorMessage)
        //                .ToArray()
        //        });
        //    }

        //    try
        //    {
        //        var result = await _variantRepository.UpdateProductApprovalAsync(id, approvalDto);
        //        return Ok(new ApiResponse<object>
        //        {
        //            Message = "Product approval status updated",
        //            Data = result,
        //            Success = true
        //        });
        //    }
        //    catch (KeyNotFoundException ex)
        //    {
        //        return NotFound(new ApiErrorResponse
        //        {
        //            Message = ex.Message,
        //            ErrorMessages = new string[] { ex.Message }
        //        });
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        return BadRequest(new ApiErrorResponse
        //        {
        //            Message = ex.Message,
        //            ErrorMessages = new string[] { ex.Message }
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new ApiErrorResponse
        //        {
        //            Message = "An error occurred while updating product approval status",
        //            ErrorMessages = new string[] { ex.Message }
        //        });
        //    }
        //}
    //}
}
