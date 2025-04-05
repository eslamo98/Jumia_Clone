using Jumia_Clone.Data;
using Jumia_Clone.Models.DTOs.SubcategoryDTOs;
using Jumia_Clone.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Jumia_Clone.Repositories
{
    public class SubcategoryRepository : ISubcategory
    {
        private readonly ApplicationDbContext _context;
        public SubcategoryRepository(ApplicationDbContext context)
        {
           
            _context = context;
        }

        public List<Subcategorydto> GetSubcategoriesByCategory(int CategoryId)
        {
            var subcategories = _context.SubCategories
             .Where(sc => sc.CategoryId == CategoryId)
             .Select(sc => new Subcategorydto
             {
                 SubcategoryId = sc.SubcategoryId,
                 Name = sc.Name,
                 CategoryId = sc.CategoryId,
                 Description = sc.Description,
                 ImageUrl = sc.ImageUrl,
                 IsActive = sc.IsActive==true,
                 ProductCount = sc.Products.Count(),

             })
             .ToList();
            return subcategories;


        }
    }
}
