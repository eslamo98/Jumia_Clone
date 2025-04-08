using Jumia_Clone.Data;
using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Models.DTOs.SubcategoryDTOs;
using Jumia_Clone.Models.Entities;
using Jumia_Clone.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Jumia_Clone.Repositories.Implementation
{
    public class SubcategoryRepository : ISubcategoryService
    {
        private readonly ApplicationDbContext _context;

        public SubcategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get Subcategories by Category
        public async Task<List<Subcategorydto>> GetSubcategoriesByCategory(int CategoryId)
        {
            var subcategories = await _context.SubCategories
                .Where(sc => sc.CategoryId == CategoryId && sc.IsActive == true) // Handle nullable IsActive
                .Select(sc => new Subcategorydto
                {
                    SubcategoryId = sc.SubcategoryId,
                    Name = sc.Name,
                    CategoryId = sc.CategoryId,
                    Description = sc.Description,
                    ImageUrl = sc.ImageUrl,
                    IsActive = sc.IsActive == true,
                    ProductCount = sc.Products.Count(),
                })
                .ToListAsync();

            return subcategories ?? new List<Subcategorydto>(); // Return an empty list if null
        }

        // Create Subcategory
        public async Task<Subcategorydto> CreateSubcategory(CreateSubcategoryDto subcategoryDto)
        {
            var subcategory = new SubCategory
            {
                Name = subcategoryDto.Name,
                CategoryId = subcategoryDto.CategoryId,
                Description = subcategoryDto.Description,
                ImageUrl = subcategoryDto.ImageUrl,
                IsActive = subcategoryDto.IsActive,
                
            };

            _context.SubCategories.Add(subcategory);
            await _context.SaveChangesAsync();


            return new Subcategorydto()
            {
                SubcategoryId = subcategory.SubcategoryId,

                CategoryId = subcategory.CategoryId,
                Description = subcategory.Description,
                ImageUrl = subcategory.ImageUrl,
                IsActive = subcategory.IsActive ?? false,
                Name = subcategory.Name,
                ProductCount = subcategory.Products.Count()
            };
            
        }

        // Update Subcategory
        public async Task<Subcategorydto> UpdateSubcategory(int subcategoryId, EditSubcategoryDto subcategoryDto)
        {

            var subcategory = await _context.SubCategories
        .FirstOrDefaultAsync(sc => sc.SubcategoryId == subcategoryId);

            if (subcategory == null)
            {
                throw new Exception("Subcategory not found");
            }
           
            subcategory.Name = subcategoryDto.Name;
            subcategory.CategoryId = subcategoryDto.CategoryId;
            subcategory.Description = subcategoryDto.Description;
            subcategory.ImageUrl = subcategoryDto.ImageUrl;
            subcategory.IsActive = subcategoryDto.IsActive;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine("DbUpdateException: " + ex.InnerException?.Message);
                throw new Exception("Error occurred while updating the subcategory.");
            }

            return new Subcategorydto
            {
                SubcategoryId = subcategory.SubcategoryId,
                Name = subcategory.Name,
                CategoryId = subcategory.CategoryId,
                Description = subcategory.Description,
                ImageUrl = subcategory.ImageUrl,
                IsActive = subcategory.IsActive ?? false,
                ProductCount = subcategory.Products?.Count ?? 0
            };
        }

        //  Delete Subcategory
        public async Task<bool> DeleteSubcategory(int subcategoryId)
        {
            var subcategory = await _context.SubCategories
                .FirstOrDefaultAsync(sc => sc.SubcategoryId == subcategoryId ); // Handle nullable bool

            if (subcategory == null)
            {
                return false; // Subcategory not found or already deleted
            }

            _context.SubCategories.Remove(subcategory);
       
            await _context.SaveChangesAsync();
            return true; // Success
        }

        // Get Subcategory by ID
        public async Task<Subcategorydto> GetSubcategoryById(int subcategoryId)
        {
            var subcategory = await _context.SubCategories
                .Where(sc => sc.SubcategoryId == subcategoryId)
                .Select(sc => new Subcategorydto
                {
                    SubcategoryId = sc.SubcategoryId,
                    Name = sc.Name,
                    CategoryId = sc.CategoryId,
                    Description = sc.Description,
                    ImageUrl = sc.ImageUrl,
                    IsActive = sc.IsActive == true,
                    ProductCount = sc.Products.Count(),
                })
                .FirstOrDefaultAsync();

            if (subcategory == null)
            {
                throw new Exception("Subcategory not found");
            }

            return subcategory;
        }


        //// Restore Soft-Deleted Subcategory
        //public async Task<Subcategorydto> RestoreSubcategory(int subcategoryId)
        //{
        //    var subcategory = await _context.SubCategories
        //        .FirstOrDefaultAsync(sc => sc.SubcategoryId == subcategoryId && sc.IsActive == false); // Find soft-deleted

        //    if (subcategory == null)
        //    {
        //        return null; // Subcategory not found or already active
        //    }

        //    subcategory.IsActive = true; // Restore by setting IsActive to true
        //    _context.SubCategories.Update(subcategory);

        //    await _context.SaveChangesAsync();

        //    var subcategoryDto = new Subcategorydto
        //    {
        //        SubcategoryId = subcategory.SubcategoryId,
        //        Name = subcategory.Name,
        //        CategoryId = subcategory.CategoryId,
        //        Description = subcategory.Description,
        //        ImageUrl = subcategory.ImageUrl,
        //        IsActive = subcategory.IsActive.HasValue, // Updated status
        //        ProductCount = subcategory.Products.Count
        //    };

        //    return subcategoryDto;
        //}
        public async Task<IEnumerable<SearchSubcategoryDto>> SearchByNameOrDescription(string searchTerm, PaginationDto pagination)
        {
            var subcategories = await _context.SubCategories
                .Where(sc => (sc.Name.Contains(searchTerm) || sc.Description.Contains(searchTerm)) && sc.IsActive==true)
                .Skip(pagination.PageSize * pagination.PageNumber).Take(pagination.PageSize)
                .Select(sc => new SearchSubcategoryDto
                {
                    Name = sc.Name,
                    Description = sc.Description,
                    IsActive = sc.IsActive ?? false,
                    ImageUrl = sc.ImageUrl,
                })
                .ToListAsync();

            return subcategories;
        }



    }
}
