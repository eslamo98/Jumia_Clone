using Jumia_Clone.Repositories.Interfaces;
using Jumia_Clone.Repositories.Implementation;

using Microsoft.AspNetCore.Mvc;

namespace Jumia_Clone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoriesController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        // GET: api/categories
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool include_inactive = false)
        {
            var categories = await _categoryRepository.GetAllCategoriesAsync(include_inactive);
            return Ok(new { success = true, data = categories });
        }
        // GET: api/categories/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(id);
            if (category == null) return NotFound(new { success = false, message = "Category not found" });

            return Ok(new { success = true, data = category });
        }
    }

    }
