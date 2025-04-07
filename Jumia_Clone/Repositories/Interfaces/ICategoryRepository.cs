using Jumia_Clone.Models.DTOs.CategoryDTO;
using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Models.Entities;

namespace Jumia_Clone.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        // Method to get all categories
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(PaginationDto pagination, bool includeInactive = false);

        // Method to get a category by ID
        Task<CategoryDto> GetCategoryByIdAsync(int id);

        // Method to create a new category
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto categoryDto);

        // Method to update an existing category
        Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryDto categoryDto);

        // Method to delete a category and its subcategories
        Task DeleteCategoryAsync(int id);
    }
}
