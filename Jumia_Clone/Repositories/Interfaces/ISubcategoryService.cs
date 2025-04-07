using Jumia_Clone.Models.DTOs.SubcategoryDTOs;
using Jumia_Clone.Models.Entities;

namespace Jumia_Clone.Repositories.Interfaces
{
    public interface ISubcategoryService
    {
        Task<List<Subcategorydto>> GetSubcategoriesByCategory(int CategoryId);
        Task<Subcategorydto> CreateSubcategory(CreateSubcategoryDto subcategoryDto);
        Task<Subcategorydto> UpdateSubcategoryAsync(int subcategoryId, EditSubcategoryDto subcategoryDto);
        Task<bool> SoftDeleteSubcategory(int subcategoryId); 
        Task<Subcategorydto> GetSubcategoryByIdAsync(int subcategoryId);
        Task<Subcategorydto> RestoreSubcategory(int subcategoryId);


    }
}

