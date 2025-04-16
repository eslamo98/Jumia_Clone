using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Models.DTOs.ProductImageDTOs;
using Jumia_Clone.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jumia_Clone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductImagesController : ControllerBase
    {
        private readonly IProductImageRepository _productImageRepository;

        public ProductImagesController(IProductImageRepository productImageRepository)
        {
            _productImageRepository = productImageRepository;
        }

        [Authorize(Roles = "seller,Admin")]
        [HttpPost("{productId}/images")]
        public async Task<IActionResult> AddProductImages(
        int productId,
        [FromForm] List<IFormFile> imageFiles)
        {
            try
            {
                var createDto = new CreateProductImageDto
                {
                    ProductId = productId,
                    ImageFiles = imageFiles
                };

                var addedImages = await _productImageRepository.AddProductImagesAsync(createDto);

                return Ok(new ApiResponse<IEnumerable<ProductImageDto>>
                {
                    Message = "Images uploaded successfully",
                    Data = addedImages,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "Error uploading images",
                    ErrorMessages = new[] { ex.Message }
                });
            }
        }

        [Authorize(Roles = "seller,Admin")]
        [HttpPut("{productId}/images")]
        public async Task<IActionResult> UpdateProductImages(
            int productId,
            [FromForm] List<IFormFile> newImageFiles,
            [FromForm] List<int> imagesToDelete)
        {
            try
            {
                var updateDto = new UpdateProductImagesDto
                {
                    ProductId = productId,
                    NewImageFiles = newImageFiles,
                    ImagesToDelete = imagesToDelete
                };

                await _productImageRepository.UpdateProductImagesAsync(updateDto);

                return Ok(new ApiResponse<object>
                {
                    Message = "Images updated successfully",
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "Error updating images",
                    ErrorMessages = new[] { ex.Message }
                });
            }
        }

        [Authorize(Roles = "seller,Admin")]
        [HttpDelete("images")]
        public async Task<IActionResult> DeleteProductImages([FromBody] List<int> imageIds)
        {
            try
            {
                await _productImageRepository.DeleteProductImagesAsync(imageIds);

                return Ok(new ApiResponse<object>
                {
                    Message = "Images deleted successfully",
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "Error deleting images",
                    ErrorMessages = new[] { ex.Message }
                });
            }
        }
    }
}
