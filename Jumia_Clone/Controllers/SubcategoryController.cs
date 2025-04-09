using Jumia_Clone.Models.DTOs.SubcategoryDTOs;
using Jumia_Clone.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Jumia_Clone.Repositories;
using Jumia_Clone.Services;
using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Repositories.Implementation;
using Microsoft.EntityFrameworkCore;

namespace Jumia_Clone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubcategoryController : ControllerBase
    {
        private readonly ISubcategoryService _subcategoryService;

        public SubcategoryController(ISubcategoryService subcategoryService)
        {
            _subcategoryService = subcategoryService;
        }

        // Get Subcategories by Category
        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetAll(int categoryId, [FromQuery] PaginationDto pagination)
        {
            try
            {
                var subcategories = await _subcategoryService.GetSubcategoriesByCategory(categoryId, pagination);

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



        // Create a Subcategory
        // POST: api/Subcategory
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSubcategoryDto subcategoryDto)
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
                var createdSubcategory = await _subcategoryService.CreateSubcategory(subcategoryDto);

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
                    Message = "There is already a subcategory with name " + subcategoryDto.Name,
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


        // Update a Subcategory
        [HttpPut("{subcategoryId}")]
        public async Task<IActionResult> Update(int subcategoryId, [FromBody] EditSubcategoryDto subcategoryDto)
        {
            try
            {
                var updatedSubcategory = await _subcategoryService.UpdateSubcategory(subcategoryId, subcategoryDto);
                if (updatedSubcategory == null)
                {
                    return NotFound(new ApiErrorResponse(
                        new[] { "Subcategory not found." },
                        "Subcategory not found."
                    ));
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

        // DELETE: api/SubCategories/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _subcategoryService.DeleteSubcategory(id);
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

        // Get Subcategory by ID
        [HttpGet("{subcategoryId}")]
        public async Task<IActionResult> GetById(int subcategoryId)
        {
            try
            {
                var subcategory = await _subcategoryService.GetSubcategoryById(subcategoryId);
                if (subcategory == null)
                {
                    return NotFound(new ApiErrorResponse(
                        new[] { "Subcategory not found." },
                        "Subcategory not found."
                    ));
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

        // Search Subcategories by Name or Description
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string searchTerm, [FromQuery] PaginationDto pagination)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return BadRequest(new ApiErrorResponse(
                        new[] { "Search term cannot be empty" },
                        "Search term is required."
                    ));
                }

                var result = await _subcategoryService.SearchByNameOrDescription(searchTerm, pagination);

                if (result != null && result.Any())
                {
                    return Ok(new ApiResponse<IEnumerable<SearchSubcategoryDto>>(
                        result,
                        "Successfully retrieved matching subcategories"
                    ));
                }

                return NotFound(new ApiErrorResponse(
                    new[] { "No matching subcategories found" },
                    "No subcategories found matching the search criteria."
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse(
                    new[] { ex.Message },
                    "An error occurred while retrieving subcategories"
                ));
            }
        }
    }
}
