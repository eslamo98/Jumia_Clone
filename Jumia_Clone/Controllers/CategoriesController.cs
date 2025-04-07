using Jumia_Clone.Repositories.Interfaces;
using Jumia_Clone.Models.DTOs.CategoryDTO;
using Microsoft.AspNetCore.Mvc;
using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Microsoft.EntityFrameworkCore;

namespace Jumia_Clone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoriesController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        // GET: api/categories
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationDto pagination ,[FromQuery] bool include_inactive = false)
        {
            try
            {
                var categories = await _categoryRepository.GetAllCategoriesAsync(pagination, include_inactive);
                return Ok(new ApiResponse<IEnumerable<CategoryDto>>
                {
                   Message = "Successfully retrieved all categories",
                   Data = categories,
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
                    return NotFound(new ApiErrorResponse() { Message = "Category not found", ErrorMessages = new[] { "Category was not found" } });
                return Ok(new ApiResponse<CategoryDto>{ Message = "Category was retreived successfully!", Data = category });
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

        // POST: api/categories
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto categoryDto)
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
                var createdCategory = await _categoryRepository.CreateCategoryAsync(categoryDto);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = createdCategory.CategoryId },
                    new ApiResponse<CategoryDto>() { Data = createdCategory, Message = "Category was created successfully!" }
                );
            }
            catch(DbUpdateException ex)
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
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto categoryDto)
        {
            if(id != categoryDto.CategoryId) return BadRequest(new ApiErrorResponse()
            {
                Message = "Invalid category id",
                ErrorMessages = new string[] { "Invalid category id"}
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
                var updatedCategory = await _categoryRepository.UpdateCategoryAsync(id, categoryDto);

                return Ok(new ApiResponse<CategoryDto>()
                {
                    Data = updatedCategory,
                    Message = $"Category was updated successfully!"
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ApiErrorResponse(){ Message = "Category not found", ErrorMessages = new[] { "Category was not found" } });
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
                await _categoryRepository.DeleteCategoryAsync(id);

                return Ok(new ApiResponse<object>
                {
                    Message = "Category and its subcategories deleted successfully",
                    Data = null
                    
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ApiErrorResponse() { Message = "Category not found", ErrorMessages = new[] { "Category was not found" } });
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