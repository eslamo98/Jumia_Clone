using Jumia_Clone.Models.DTOs.SubcategoryDTOs;
using Jumia_Clone.Repositories.Interfaces;

namespace Jumia_Clone.Services
{
    public class SubcategoryService
    {
        private readonly ISubcategoryService _subcategoryRepository;

        public SubcategoryService(ISubcategoryService subcategoryRepository)
        {
            _subcategoryRepository = subcategoryRepository;
        }

        public async Task<List<Subcategorydto>> GetSubcategoriesByCategory(int categoryId)
        {
            return await _subcategoryRepository.GetSubcategoriesByCategory(categoryId);
        }

        public async Task<Subcategorydto> CreateSubcategory(Subcategorydto subcategoryDto)
        {
            return await _subcategoryRepository.CreateSubcategory(subcategoryDto);
        }

        public async Task<Subcategorydto> UpdateSubcategoryAsync(int subcategoryId, Subcategorydto subcategoryDto)
        {
            return await _subcategoryRepository.UpdateSubcategoryAsync(subcategoryId, subcategoryDto);
        }

        public async Task<bool> SoftDeleteSubcategory(int subcategoryId)
        {
            return await _subcategoryRepository.SoftDeleteSubcategory(subcategoryId);
        }

        public async Task<Subcategorydto> GetSubcategoryByIdAsync(int subcategoryId)
        {
            return await _subcategoryRepository.GetSubcategoryByIdAsync(subcategoryId);
        }

        public async Task<Subcategorydto> RestoreSubcategory(int subcategoryId)
        {
            return await _subcategoryRepository.RestoreSubcategory(subcategoryId);
        }
    }
}
