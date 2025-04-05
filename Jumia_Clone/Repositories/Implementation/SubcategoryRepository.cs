using Jumia_Clone.Data;
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
        public async Task<Subcategorydto> CreateSubcategory(Subcategorydto subcategoryDto)
        {
            var subcategory = new SubCategory
            {
                Name = subcategoryDto.Name,
               // CategoryId = subcategoryDto.CategoryId,
                Description = subcategoryDto.Description,
                ImageUrl = subcategoryDto.ImageUrl,
                IsActive = subcategoryDto.IsActive
            };

            _context.SubCategories.Add(subcategory);
            await _context.SaveChangesAsync();

          subcategoryDto.SubcategoryId = subcategory.SubcategoryId; // Set the created SubcategoryId
            return subcategoryDto;
        }

        // Update Subcategory
        public async Task<Subcategorydto> UpdateSubcategoryAsync(int subcategoryId, Subcategorydto subcategoryDto)
        {
            var subcategory = await _context.SubCategories.FirstOrDefaultAsync(sc => sc.SubcategoryId == subcategoryId);
            if (subcategory == null)
            {
                throw new Exception("Subcategory not found");
            }

            // Update subcategory's fields
            subcategory.Name = subcategoryDto.Name;
            subcategory.CategoryId = subcategoryDto.CategoryId;
            subcategory.Description = subcategoryDto.Description;
            subcategory.ImageUrl = subcategoryDto.ImageUrl;
            subcategory.IsActive = subcategoryDto.IsActive;

            await _context.SaveChangesAsync();

            return subcategoryDto;
        }

        // Soft Delete Subcategory
        public async Task<bool> SoftDeleteSubcategory(int subcategoryId)
        {
            var subcategory = await _context.SubCategories
                .FirstOrDefaultAsync(sc => sc.SubcategoryId == subcategoryId && (sc.IsActive ?? false)); // Handle nullable bool

            if (subcategory == null)
            {
                return false; // Subcategory not found or already deleted
            }

            subcategory.IsActive = false; // Mark as inactive (soft delete)
            _context.SubCategories.Update(subcategory);

            await _context.SaveChangesAsync();
            return true; // Success
        }

        // Get Subcategory by ID
        public async Task<Subcategorydto> GetSubcategoryByIdAsync(int subcategoryId)
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

        // Restore Soft-Deleted Subcategory
        public async Task<Subcategorydto> RestoreSubcategory(int subcategoryId)
        {
            var subcategory = await _context.SubCategories
                .FirstOrDefaultAsync(sc => sc.SubcategoryId == subcategoryId && sc.IsActive == false); // Find soft-deleted

            if (subcategory == null)
            {
                return null; // Subcategory not found or already active
            }

            subcategory.IsActive = true; // Restore by setting IsActive to true
            _context.SubCategories.Update(subcategory);

            await _context.SaveChangesAsync();

            var subcategoryDto = new Subcategorydto
            {
                SubcategoryId = subcategory.SubcategoryId,
                Name = subcategory.Name,
                CategoryId = subcategory.CategoryId,
                Description = subcategory.Description,
                ImageUrl = subcategory.ImageUrl,
                IsActive = subcategory.IsActive.HasValue, // Updated status
                ProductCount = subcategory.Products.Count
            };

            return subcategoryDto;
        }
    }
}
