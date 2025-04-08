using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Models.DTOs.SubcategoryDTOs;
using Jumia_Clone.Models.Entities;

namespace Jumia_Clone.Repositories.Interfaces
{
    public interface ISubcategoryService
    {
        Task<List<Subcategorydto>> GetSubcategoriesByCategory(int CategoryId);
        Task<Subcategorydto> CreateSubcategory(CreateSubcategoryDto subcategoryDto);
        Task<Subcategorydto> UpdateSubcategory(int subcategoryId, EditSubcategoryDto subcategoryDto);
        Task<bool> DeleteSubcategory(int subcategoryId); 
        Task<Subcategorydto> GetSubcategoryById(int subcategoryId);
        // Task<Subcategorydto> RestoreSubcategory(int subcategoryId);
        Task<IEnumerable<SearchSubcategoryDto>> SearchByNameOrDescription(string searchTerm, PaginationDto pagination);



    }
}

