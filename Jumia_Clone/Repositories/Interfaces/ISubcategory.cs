using Jumia_Clone.Models.DTOs.SubcategoryDTOs;
using Jumia_Clone.Models.Entities;

namespace Jumia_Clone.Repositories.Interfaces
{
    public interface ISubcategory
    {
        List<Subcategorydto> GetSubcategoriesByCategory(int CategoryId);

        
    }
}

