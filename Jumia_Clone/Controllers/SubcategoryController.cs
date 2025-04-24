using Jumia_Clone.Models.DTOs.SubcategoryDTOs;
using Jumia_Clone.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Jumia_Clone.Services.Interfaces;
using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Microsoft.EntityFrameworkCore;
using Jumia_Clone.Models.DTOs.ProductDTOs;
using Jumia_Clone.Repositories.Implementation;
using Jumia_Clone.Models.Enums;

namespace Jumia_Clone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubcategoryController : ControllerBase
    {
        private readonly ISubcategoryRepository _subcategoryRepository;
        private readonly IImageService _imageService;

        public SubcategoryController(ISubcategoryRepository subcategoryRepository, IImageService imageService)
        {
            _subcategoryRepository = subcategoryRepository;
            _imageService = imageService;
        }

        // GET: api/Subcategory/category/{categoryId}
        // Get all subcategories of a specific category
        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetAll(int categoryId, [FromQuery] PaginationDto pagination)
        {
            try
            {
                var subcategories = await _subcategoryRepository.GetSubcategoriesByCategory(categoryId, pagination);

                // Set full image URLs
                foreach (var subcategory in subcategories)
                {
                    if (!string.IsNullOrEmpty(subcategory.ImageUrl))
                    {
                        subcategory.ImageUrl = _imageService.GetImageUrl(subcategory.ImageUrl);
                    }
                }

                return Ok(new ApiResponse<IEnumerable<Subcategorydto>>
                {
                    Message = "Successfully retrieved subcategories.",
                    Data = subcategories,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse()
                {
                    Message = "An error occurred while retrieving subcategories.",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }

        // GET: api/Subcategory/categories/subcategory
        // Get all subcategories (with optional inactive flag)
        [HttpGet]
        public async Task<IActionResult> GetAllSubcategories([FromQuery] PaginationDto pagination, [FromQuery] bool include_inactive = false)
        {
            try
            {
                var subcategories = await _subcategoryRepository.GetAllSubcategoriesAsync(pagination, include_inactive);
                var totalItems = await _subcategoryRepository.GetCountAsync();
                foreach (var subcategory in subcategories)
                {
                    if (!string.IsNullOrEmpty(subcategory.ImageUrl))
                    {
                        subcategory.ImageUrl = _imageService.GetImageUrl(subcategory.ImageUrl);
                    }
                }

                return Ok(new 
                { 
                    Message = "Successfully retrieved all subcategories.",
                    Data = subcategories,
                    Success = true,
                    totalItems = totalItems

                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse(
                    new[] { ex.Message },
                    "An error occurred while retrieving subcategories."
                ));
            }
        }

        // GET: api/Subcategory/{subcategoryId}
        // Get subcategory by ID
        [HttpGet("{subcategoryId}")]
        public async Task<IActionResult> GetById(int subcategoryId)
        {
            try
            {
                var subcategory = await _subcategoryRepository.GetSubcategoryById(subcategoryId);

                if (subcategory == null)
                {
                    return NotFound(new ApiErrorResponse(
                        new[] { "Subcategory not found." },
                        "Subcategory not found."
                    ));
                }

                if (!string.IsNullOrEmpty(subcategory.ImageUrl))
                {
                    subcategory.ImageUrl = _imageService.GetImageUrl(subcategory.ImageUrl);
                }

                return Ok(new ApiResponse<Subcategorydto>(
                    subcategory,
                    "Successfully retrieved subcategory."
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse(
                    new[] { ex.Message },
                    "An error occurred while retrieving the subcategory."
                ));
            }
        }

        // POST: api/Subcategory
        // Create a new subcategory
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] CreateSubcategoryDto subcategoryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "Invalid subcategory data",
                    ErrorMessages = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToArray()
                });
            }

            try
            {
                var createdSubcategory = await _subcategoryRepository.CreateSubcategory(subcategoryDto);

                if (subcategoryDto.ImageFile != null && subcategoryDto.ImageFile.Length > 0)
                {
                    string entityName = $"{createdSubcategory.Name}-{createdSubcategory.SubcategoryId}";
                    string imagePath = await _imageService.SaveImageAsync(
                        subcategoryDto.ImageFile,
                        EntityType.SubCategory,
                        entityName
                    );
                    // Update the product with the image path
                    createdSubcategory = await _subcategoryRepository.UpdateSubcategoryImageAsync(
                        createdSubcategory.SubcategoryId,
                        imagePath
                    );
                    // Add the image URL to the response
                    createdSubcategory.ImageUrl = _imageService.GetImageUrl(imagePath);
                }

                return CreatedAtAction(
                    nameof(GetById),
                    new { subcategoryId = createdSubcategory.SubcategoryId },
                    new ApiResponse<Subcategorydto>
                    {
                        Data = createdSubcategory,
                        Message = "Subcategory was created successfully!"
                    });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "There is already a subcategory with the name " + subcategoryDto.Name,
                    ErrorMessages = new[] { ex.Message }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while creating the subcategory",
                    ErrorMessages = new[] { ex.Message }
                });
            }
        }

        // PUT: api/Subcategory/{subcategoryId}
        // Update an existing subcategory
        [HttpPut("{subcategoryId}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(int subcategoryId, [FromForm] EditSubcategoryDto subcategoryDto)
        {
            try
            {
                var existingSubCategory = await _subcategoryRepository.GetSubcategoryById(subcategoryDto.SubcategoryId);
                if (existingSubCategory == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Message = "Subcategory not found",
                        Success = false,
                        Data = null
                    });
                }
                var updatedSubcategory = await _subcategoryRepository.UpdateSubcategory(subcategoryId, subcategoryDto);

                if (updatedSubcategory == null)
                {
                    return NotFound(new ApiErrorResponse(
                        new[] { "Subcategory not found." },
                        "Subcategory not found."
                    ));
                }

                // Handle main image update if provided
                if (subcategoryDto.ImageFile != null && subcategoryDto.ImageFile.Length > 0)
                {
                    string entityName = $"{updatedSubcategory.Name}-{updatedSubcategory.SubcategoryId}";
                    string imagePath = await _imageService.UpdateImageAsync(
                        subcategoryDto.ImageFile,
                        existingSubCategory.ImageUrl,
                        EntityType.Product,
                        entityName
                    );

                    // Update the product with the new image path
                    updatedSubcategory = await _subcategoryRepository.UpdateSubcategoryImageAsync(updatedSubcategory.SubcategoryId, imagePath);

                    // Add the image URL to the response
                    updatedSubcategory.ImageUrl = _imageService.GetImageUrl(imagePath);
                }

               

                return Ok(new ApiResponse<Subcategorydto>(
                    updatedSubcategory,
                    "Subcategory updated successfully."
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse(
                    new[] { ex.Message },
                    "An error occurred while updating the subcategory."
                ));
            }
        }

        // DELETE: api/Subcategory/{id}
        // delete a subcategory
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _subcategoryRepository.DeleteSubcategory(id);

                return Ok(new ApiResponse<object>(
                    null,
                    "Subcategory deleted successfully."
                ));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ApiErrorResponse(
                    new[] { "Subcategory was not found." },
                    "Subcategory not found."
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse(
                    new[] { ex.Message },
                    "An error occurred while deleting the subcategory."
                ));
            }
        }

        // GET: api/Subcategory/search
        // Search subcategories by name or description
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string searchTerm, [FromQuery] PaginationDto pagination)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return BadRequest(new ApiErrorResponse(
                        new[] { "Search term cannot be empty." },
                        "Search term is required."
                    ));
                }

                var result = await _subcategoryRepository.SearchByNameOrDescription(searchTerm, pagination);

                foreach (var subcategory in result)
                {
                    if (!string.IsNullOrEmpty(subcategory.ImageUrl))
                    {
                        subcategory.ImageUrl = _imageService.GetImageUrl(subcategory.ImageUrl);
                    }
                }

                if (result != null && result.Any())
                {
                    return Ok(new ApiResponse<IEnumerable<SearchSubcategoryDto>>(
                        result,
                        "Successfully retrieved matching subcategories."
                    ));
                }

                return NotFound(new ApiErrorResponse(
                    new[] { "No matching subcategories found." },
                    "No subcategories found matching the search criteria."
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse(
                    new[] { ex.Message },
                    "An error occurred while searching for subcategories."
                ));
            }
        }

        // GET: api/Subcategory/basic-info
        [HttpGet("basic-info/{categoryId}")]
        public async Task<IActionResult> GetBasicInfo(int categoryId)
        {
            try
            {
                var subcategories = await _subcategoryRepository.GetBasicInfo(categoryId);
                return Ok(new ApiResponse<IEnumerable<SubcategoryBasicInfoDto>>
                {
                    Message = "Successfully retrieved basic subcategory information",
                    Data = subcategories,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving basic subcategory information",
                    ErrorMessages = new string[] { ex.Message }
                });
            }
        }
    }
}
