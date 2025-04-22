using Jumia_Clone.Repositories.Interfaces;
using Jumia_Clone.Models.DTOs.CategoryDTO;
using Microsoft.AspNetCore.Mvc;
using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Microsoft.EntityFrameworkCore;
using Jumia_Clone.Services.Interfaces;
using Jumia_Clone.Models.Enums;

namespace Jumia_Clone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IImageService _imageService;

        public CategoriesController(ICategoryRepository categoryRepository, IImageService imageService)
        {
            _categoryRepository = categoryRepository;
            _imageService = imageService;
        }

        // GET: api/categories
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationDto pagination, [FromQuery] bool include_inactive = false)
        {
            try
            {
                var categories = await _categoryRepository.GetAllCategoriesAsync(pagination, include_inactive);
                var totalItems = await _categoryRepository.GetCount();
                // Convert image paths to URLs if any
                foreach (var category in categories)
                {
                    if (!string.IsNullOrEmpty(category.ImageUrl))
                    {
                        category.ImageUrl = _imageService.GetImageUrl(category.ImageUrl);
                    }
                }

                return Ok(new
                {
                    Message = "Successfully retrieved all categories",
                    Data = categories,
                    totalItems = totalItems,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse()
                {
                    Message = "An error occurred while retrieving categories",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        [HttpGet("Search")]
        public async Task<IActionResult> Search([FromQuery] string searchTerm, [FromQuery] bool include_inactive = false)
        {
            try
            {
                var categories = await _categoryRepository.SearchCategoriesAsync(searchTerm, include_inactive);
                var totalItems = await _categoryRepository.GetCount();
                // Convert image paths to URLs if any
                foreach (var category in categories)
                {
                    if (!string.IsNullOrEmpty(category.ImageUrl))
                    {
                        category.ImageUrl = _imageService.GetImageUrl(category.ImageUrl);
                    }
                }

                return Ok(new
                {
                    Message = "Successfully retrieved all categories",
                    Data = categories,
                    totalItems = totalItems,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse()
                {
                    Message = "An error occurred while retrieving categories",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // GET: api/categories/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var category = await _categoryRepository.GetCategoryByIdAsync(id);

                if (category == null)
                    return NotFound(new ApiResponse<object>()
                    {
                        Message = "Category not found",
                        Success = false,
                        Data = null
                    });

                // Add image URL if image path exists
                if (!string.IsNullOrEmpty(category.ImageUrl))
                {
                    category.ImageUrl = _imageService.GetImageUrl(category.ImageUrl);
                }

                return Ok(new ApiResponse<CategoryDto>
                {
                    Message = "Category was retrieved successfully!",
                    Data = category,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse()
                {
                    Message = "An error occurred while retrieving category with id = " + id,
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // GET: api/categories/basic-info
        [HttpGet("basic-info")]
        public async Task<IActionResult> GetBasicInfo()
        {
            try
            {
                var categories = await _categoryRepository.GetBasicInfo();
                return Ok(new ApiResponse<IEnumerable<CategoryBasicInfoDto>>
                {
                    Message = "Successfully retrieved basic category information",
                    Data = categories,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving basic category information",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // POST: api/categories
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] CreateCategoryInputDto categoryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiErrorResponse()
                {
                    Message = "Invalid category data",
                    ErrorMessages = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage).ToArray()
                });
            }

            try
            {
                // First create the category without image
                var createdCategory = await _categoryRepository.CreateCategoryAsync(categoryDto);

                // Then handle image upload if provided
                if (categoryDto.ImageFile != null && categoryDto.ImageFile.Length > 0)
                {
                    // Create folder structure based on category ID and name
                    string entityName = $"{createdCategory.Name}-{createdCategory.CategoryId}";

                    // Save the image
                    string imagePath = await _imageService.SaveImageAsync(
                        categoryDto.ImageFile,
                        EntityType.Category,
                        entityName
                    );

          
                    var dbUpdateModel = new
                    {
                        CategoryId = createdCategory.CategoryId,
                        Name = createdCategory.Name,
                        Description = createdCategory.Description,
                        IsActive = createdCategory.IsActive,
                        ImageUrl = imagePath // This maps to the DB ImageUrl column
                    };

                    // Update the category with the image path in database
                    createdCategory = await _categoryRepository.UpdateCategoryImageAsync(
                        createdCategory.CategoryId,
                        imagePath
                    );

                    // Add the image URL to the response
                    createdCategory.ImageUrl = _imageService.GetImageUrl(imagePath);
                }

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = createdCategory.CategoryId },
                    new ApiResponse<CategoryDto>()
                    {
                        Data = createdCategory,
                        Message = "Category was created successfully!",
                        Success = true
                    }
                );
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new ApiErrorResponse()
                {
                    Message = "There is already a category with name " + categoryDto.Name,
                    ErrorMessages = new string[] { ex.Message }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse()
                {
                    Message = "An error occurred while creating the category",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // PUT: api/categories/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateCategoryInputDto categoryDto)
        {
            if (id != categoryDto.CategoryId)
                return BadRequest(new ApiErrorResponse()
                {
                    Message = "Invalid category id",
                    ErrorMessages = new string[] { "Invalid category id" }
                });

            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiErrorResponse()
                {
                    Message = "Invalid category data",
                    ErrorMessages = ModelState.Values
                       .SelectMany(v => v.Errors)
                       .Select(e => e.ErrorMessage).ToArray()
                });
            }

            try
            {
                // Get existing category to check if it exists and to get the current image path
                var existingCategory = await _categoryRepository.GetCategoryByIdAsync(id);
                if (existingCategory == null)
                {
                    return NotFound(new ApiResponse<object>()
                    {
                        Message = "Category not found",
                        Success = false,
                        Data = null
                    });
                }

                string imagePath = existingCategory.ImageUrl;

                // Handle image update if a new image was provided
                if (categoryDto.ImageFile != null && categoryDto.ImageFile.Length > 0)
                {
                    // Create folder structure based on category ID and name
                    string entityName = $"{categoryDto.Name}-{id}";

                    // Update the image (delete old one and save new one)
                    imagePath = await _imageService.UpdateImageAsync(
                        categoryDto.ImageFile,
                        existingCategory.ImageUrl,
                        EntityType.Category,
                        entityName
                    );
                }

                // Update the category basic info
                var updatedCategory = await _categoryRepository.UpdateCategoryAsync(id, categoryDto);

                // Update image path separately if needed
                if (categoryDto.ImageFile != null && categoryDto.ImageFile.Length > 0)
                {
                    updatedCategory = await _categoryRepository.UpdateCategoryImageAsync(id, imagePath);
                }

                // Add image URL to response
                if (!string.IsNullOrEmpty(updatedCategory.ImageUrl))
                {
                    updatedCategory.ImageUrl = _imageService.GetImageUrl(updatedCategory.ImageUrl);
                }

                return Ok(new ApiResponse<CategoryDto>()
                {
                    Data = updatedCategory,
                    Message = $"Category was updated successfully!",
                    Success = true
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ApiResponse<object>()
                {
                    Message = "Category not found",
                    Success = false,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse()
                {
                    Message = "An error occurred while updating the category",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // DELETE: api/categories/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Get the category to find its image path
                var category = await _categoryRepository.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return NotFound(new ApiResponse<object>()
                    {
                        Message = "Category not found",
                        Success = false,
                        Data = null
                    });
                }

                // Delete the category image if it exists
                if (!string.IsNullOrEmpty(category.ImageUrl))
                {
                    await _imageService.DeleteImageAsync(category.ImageUrl);
                }

                // Delete the category from the database
                await _categoryRepository.DeleteCategoryAsync(id);

                return Ok(new ApiResponse<object>
                {
                    Message = "Category and its subcategories deleted successfully",
                    Data = null,
                    Success = true
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ApiResponse<object>()
                {
                    Message = "Category not found",
                    Success = false,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse()
                {
                    Message = "An error occurred while deleting the category",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }
    }
}