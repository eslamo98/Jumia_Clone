using Jumia_Clone.Models.DTOs.CategoryDTO;
using Jumia_Clone.Models.DTOs.GeneralDTOs;

namespace Jumia_Clone.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(PaginationDto pagination, bool includeInactive = false);
        Task<CategoryDto> GetCategoryByIdAsync(int id);
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryInputDto categoryDto);
        Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryInputDto categoryDto);
        Task<CategoryDto> UpdateCategoryImageAsync(int id, string imagePath);
        Task DeleteCategoryAsync(int id);
    }
}