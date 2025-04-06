using System;
using Jumia_Clone.Data;
using Jumia_Clone.Models.DTOs.CategoryDTO;
using Jumia_Clone.Models.Entities;
using Jumia_Clone.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Jumia_Clone.Repositories.Implementation
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        // Get all categories
        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(bool includeInactive = false)
        {
            var categories = await _context.Categories
                .Where(c => includeInactive || c.IsActive == true)
                .Select(c => new CategoryDto
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Description = c.Description,
                    ImageUrl = c.ImageUrl,
                    IsActive = c.IsActive ?? false
                })
                .ToListAsync();

            return categories;
        }
        // Get a category by ID
        public async Task<CategoryDto> GetCategoryByIdAsync(int id)
        {
            var category = await _context.Categories
                .Where(c => c.CategoryId == id)
                .Select(c => new CategoryDto
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Description = c.Description,
                    ImageUrl = c.ImageUrl,
                    IsActive = c.IsActive ?? false
                })
                .FirstOrDefaultAsync();

            return category;
        }
        // Create a new category
        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto categoryDto)
        {
            var category = new Category
            {
                Name = categoryDto.Name,
                Description = categoryDto.Description,
                ImageUrl = categoryDto.ImageUrl,
                IsActive = categoryDto.IsActive
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            // Return the newly created category as a DTO
            return new CategoryDto
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description,
                ImageUrl = category.ImageUrl,
                IsActive = category.IsActive ?? false
            };
        }
        // Update an existing category
        public async Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryDto categoryDto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) throw new KeyNotFoundException("Category not found");

            category.Name = categoryDto.Name;
            category.Description = categoryDto.Description;
            category.ImageUrl = categoryDto.ImageUrl;
            category.IsActive = categoryDto.IsActive;

            await _context.SaveChangesAsync();
            // Return the updated category as a DTO
            return new CategoryDto
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description,
                ImageUrl = category.ImageUrl,
                IsActive = category.IsActive ?? false
            };
        }
    }
}
