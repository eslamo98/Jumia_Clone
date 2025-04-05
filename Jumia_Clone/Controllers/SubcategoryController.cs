using Jumia_Clone.Models.DTOs.SubcategoryDTOs;
using Jumia_Clone.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Jumia_Clone.Repositories;
using Jumia_Clone.Services;

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
        public async Task<ActionResult<List<Subcategorydto>>> GetSubcategoriesByCategory(int categoryId)
        {
            var subcategories = await _subcategoryService.GetSubcategoriesByCategory(categoryId);
            if (subcategories == null || subcategories.Count == 0)
            {
                return NotFound("No subcategories found for this category.");
            }
            return Ok(subcategories);
        }

        // Create a Subcategory
        [HttpPost]
        public async Task<ActionResult<Subcategorydto>> CreateSubcategory([FromBody] Subcategorydto subcategoryDto)
        {
            if (subcategoryDto == null)
            {
                return BadRequest("Invalid data.");
            }

            var createdSubcategory = await _subcategoryService.CreateSubcategory(subcategoryDto);
            return CreatedAtAction(nameof(GetSubcategoryById), new { subcategoryId = createdSubcategory.SubcategoryId }, createdSubcategory);
        }

        // Update a Subcategory
        [HttpPut("{subcategoryId}")]
        public async Task<ActionResult<Subcategorydto>> UpdateSubcategory(int subcategoryId, [FromBody] Subcategorydto subcategoryDto)
        {
            var updatedSubcategory = await _subcategoryService.UpdateSubcategoryAsync(subcategoryId, subcategoryDto);
            if (updatedSubcategory == null)
            {
                return NotFound("Subcategory not found.");
            }
            return Ok(updatedSubcategory);
        }

        // Soft Delete a Subcategory
        [HttpDelete("{subcategoryId}")]
        public async Task<ActionResult<bool>> SoftDeleteSubcategory(int subcategoryId)
        {
            var result = await _subcategoryService.SoftDeleteSubcategory(subcategoryId);
            if (!result)
            {
                return NotFound("Subcategory not found or already deleted.");
            }
            return Ok(true);
        }

        // Restore a Soft-Deleted Subcategory
        [HttpPut("restore/{subcategoryId}")]
        public async Task<ActionResult<Subcategorydto>> RestoreSubcategory(int subcategoryId)
        {
            var restoredSubcategory = await _subcategoryService.RestoreSubcategory(subcategoryId);
            if (restoredSubcategory == null)
            {
                return NotFound("Subcategory not found or already active.");
            }
            return Ok(restoredSubcategory);
        }

        // Get Subcategory by ID
        [HttpGet("{subcategoryId}")]
        public async Task<ActionResult<Subcategorydto>> GetSubcategoryById(int subcategoryId)
        {
            var subcategory = await _subcategoryService.GetSubcategoryByIdAsync(subcategoryId);
            if (subcategory == null)
            {
                return NotFound("Subcategory not found.");
            }
            return Ok(subcategory);
        }
    }
}
