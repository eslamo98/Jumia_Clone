using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Models.DTOs.ProductAttributeDTOs;
using Jumia_Clone.Models.DTOs.ProductAttributeValueDTOs;

namespace Jumia_Clone.Repositories.Interfaces
{
    public interface IProductAttributeRepository
    {
        // ProductAttribute Methods
        Task<ProductAttributeDto> CreateProductAttributeAsync(CreateProductAttributeDto attributeDto);
        Task<ProductAttributeDto> UpdateProductAttributeAsync(int attributeId, UpdateProductAttributeDto attributeDto);
        Task DeleteProductAttributeAsync(int attributeId);
        Task<ProductAttributeDto> GetProductAttributeByIdAsync(int attributeId);
        Task<IEnumerable<ProductAttributeDto>> GetProductAttributesBySubcategoryAsync(int subcategoryId);
        Task<List<ProductAttributeDto>> GetProductAttributes(PaginationDto pagination, bool include_details);
        Task<int> GetCountAsync();
        // ProductAttributeValue Methods
        Task<ProductAttributeValueDto> AddProductAttributeValueAsync(CreateProductAttributeValueDto attributeValueDto);
        Task<ProductAttributeValueDto> UpdateProductAttributeValueAsync(int valueId, UpdateProductAttributeValueDto attributeValueDto);
        Task DeleteProductAttributeValueAsync(int valueId);
        Task<IEnumerable<ProductAttributeValueDto>> GetProductAttributeValuesAsync(int productId);
    }
}
