using Jumia_Clone.Models.DTOs.CategoryDTO;
using Jumia_Clone.Models.Entities;

namespace Jumia_Clone.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        // Method to get all categories
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(bool includeInactive = false);

        // Method to get a category by ID
        Task<CategoryDto> GetCategoryByIdAsync(int id);

        // Method to create a new category
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto categoryDto);

        // Method to update an existing category
        Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryDto categoryDto);
    }
}
