using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Models.DTOs.ProductDTOs;
using Jumia_Clone.Models.DTOs.SubcategoryDTOs;
using Jumia_Clone.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Jumia_Clone.Repositories.Interfaces
{
    public interface ISubcategoryRepository
    {
        Task<IEnumerable<Subcategorydto>> GetSubcategoriesByCategory(int categoryId, PaginationDto paginationDto);

        Task<Subcategorydto> CreateSubcategory(CreateSubcategoryDto subcategoryDto);
        Task<Subcategorydto> UpdateSubcategory(int subcategoryId, EditSubcategoryDto subcategoryDto);
        Task DeleteSubcategory(int subcategoryId);
        Task<Subcategorydto> GetSubcategoryById(int subcategoryId);
        // Task<Subcategorydto> RestoreSubcategory(int subcategoryId);
        Task<IEnumerable<SearchSubcategoryDto>> SearchByNameOrDescription(string searchTerm, PaginationDto pagination);
        Task<IEnumerable<Subcategorydto>> GetAllSubcategoriesAsync(PaginationDto pagination, bool includeInactive);
        Task<IEnumerable<SubcategoryBasicInfoDto>> GetBasicInfo(int categoryId);
        Task<int> GetCountAsync();
        Task<Subcategorydto> UpdateSubcategoryImageAsync(int id, string imagePath);

    }
}

