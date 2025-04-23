using System;
using Jumia_Clone.Data;
using Jumia_Clone.Models.DTOs.CategoryDTO;
using Jumia_Clone.Models.DTOs.GeneralDTOs;
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

        public async Task<int> GetCount()
        {
            return await _context.Categories.CountAsync();
        }
        // Get all categories
        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(PaginationDto pagination, bool includeInactive = false)
        {
            var categories = await _context.Categories.Skip(pagination.PageSize  * (pagination.PageNumber - 1)).Take(pagination.PageSize)
                .Where(c => includeInactive || c.IsActive == true)
                .Select(c => new CategoryDto
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Description = c.Description,
                    ImageUrl = c.ImageUrl, // Map DB ImageUrl to DTO ImageUrl
                    ProductCount = c.SubCategories.Sum(sc => sc.Products.Count),
                    IsActive = c.IsActive ?? false,
                    SubcategoryCount = c.SubCategories.Count(sc => includeInactive || sc.IsActive == true)
                })
                .ToListAsync();
            return categories;
        }

        public async Task<IEnumerable<CategoryDto>> SearchCategoriesAsync(string searchTerm, bool includeInactive = false)
        {
            var categories = await _context.Categories
                .Where(c =>
                    (includeInactive || c.IsActive == true) &&
                    (string.IsNullOrEmpty(searchTerm) || c.Name.Contains(searchTerm) || c.Description.Contains(searchTerm)))
                
                .Select(c => new CategoryDto
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Description = c.Description,
                    ImageUrl = c.ImageUrl,
                    ProductCount = c.SubCategories.Sum(sc => sc.Products.Count),
                    IsActive = c.IsActive ?? false,
                    SubcategoryCount = c.SubCategories.Count(sc => includeInactive || sc.IsActive == true)
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
                    ImageUrl = c.ImageUrl, // Map DB ImageUrl to DTO ImageUrl
                    IsActive = c.IsActive ?? false,
                    SubcategoryCount = c.SubCategories.Count()
                })
                .FirstOrDefaultAsync();
            return category;
        }

        // Create a new category
        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryInputDto categoryDto)
        {
            var category = new Category
            {
                Name = categoryDto.Name,
                Description = categoryDto.Description,
                IsActive = categoryDto.IsActive
            };

            try
            {
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                // Return the newly created category as a DTO
                return new CategoryDto
                {
                    CategoryId = category.CategoryId,
                    Name = category.Name,
                    Description = category.Description,
                    ImageUrl = category.ImageUrl, // Map DB ImageUrl to DTO ImageUrl
                    IsActive = category.IsActive ?? false,
                    SubcategoryCount = 0
                };
            }
            catch (DbUpdateException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new Exception("Unknown Error");
            }
        }

        // Update an existing category's basic information
        public async Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryInputDto categoryDto)
        {
            var category = await _context.Categories
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null) throw new KeyNotFoundException("Category not found");

            category.Name = categoryDto.Name;
            category.Description = categoryDto.Description;
            category.IsActive = categoryDto.IsActive;
            // Note: This method doesn't update the ImageUrl

            await _context.SaveChangesAsync();

            // Return the updated category as a DTO
            return new CategoryDto
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description,
                ImageUrl = category.ImageUrl, // Map DB ImageUrl to DTO ImageUrl
                IsActive = category.IsActive ?? false,
                SubcategoryCount = category.SubCategories.Count()
            };
        }

        // Update only the image path of a category
        public async Task<CategoryDto> UpdateCategoryImageAsync(int id, string imagePath)
        {
            var category = await _context.Categories
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null) throw new KeyNotFoundException("Category not found");

            // Update only the image path
            category.ImageUrl = imagePath; // Map DTO ImageUrl to DB ImageUrl

            await _context.SaveChangesAsync();

            // Return the updated category as a DTO
            return new CategoryDto
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description,
                ImageUrl = category.ImageUrl, // Map DB ImageUrl to DTO ImageUrl
                IsActive = category.IsActive ?? false,
                SubcategoryCount = category.SubCategories.Count()
            };
        }

        // Delete a category and its subcategories in a transaction
        public async Task DeleteCategoryAsync(int id)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Find the category
                    var category = await _context.Categories
                        .Include(c => c.SubCategories)
                        .FirstOrDefaultAsync(c => c.CategoryId == id);

                    if (category == null)
                        throw new KeyNotFoundException("Category not found");

                    // Remove all associated subcategories
                    _context.SubCategories.RemoveRange(category.SubCategories);

                    // Remove the category itself
                    _context.Categories.Remove(category);

                    // Save changes
                    await _context.SaveChangesAsync();

                    // Commit transaction
                    await transaction.CommitAsync();
                }
                catch
                {
                    // Rollback the transaction if any error occurs
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<IEnumerable<CategoryBasicInfoDto>> GetBasicInfo()
        {
            return await _context.Categories
                .Where(c => c.IsActive == true)
                .Select(c => new CategoryBasicInfoDto
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name
                })
                .ToListAsync();
        }
    }
}