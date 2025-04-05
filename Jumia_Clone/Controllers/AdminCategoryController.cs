using Jumia_Clone.Models.DTOs.CategoryDTO;
using Jumia_Clone.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Jumia_Clone.Controllers
{
    [Route("api/admin/[controller]")]
    [ApiController]
    public class AdminCategoryController : Controller
    {

        private readonly ICategoryRepository _categoryRepository;

        public AdminCategoryController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }
        // POST: api/admin/categories
        [HttpPost("categories")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto categoryDto)
        {
            var category = await _categoryRepository.CreateCategoryAsync(categoryDto);
            return Ok(new { success = true, message = "Category created successfully", data = category });
        }

        // PUT: api/admin/categories/{id}
        [HttpPut("categories/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto categoryDto)
        {
            var category = await _categoryRepository.UpdateCategoryAsync(id, categoryDto);
            return Ok(new { success = true, message = "Category updated successfully", data = category });
        }
    }
}
