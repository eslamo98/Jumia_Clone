using Jumia_Clone.Data;
using Jumia_Clone.Models.DTOs.SubcategoryDTOs;
using Jumia_Clone.Models.Entities;
using Jumia_Clone.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Jumia_Clone.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Jumia_Clone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubcategoryController : ControllerBase
    {
        private readonly ISubcategory _subcategoryRepo;

        public SubcategoryController(ISubcategory subcategoryRepo)
        {
            _subcategoryRepo = subcategoryRepo;
        }

        [HttpGet("by-category")]
        public IActionResult GetSubcategoriesByCategory([FromQuery] int categoryId)
        {
            var subcategories = _subcategoryRepo.GetSubcategoriesByCategory(categoryId);

            return Ok(new
            {
                success = true,
                data = subcategories
            });





        }
    }
}

