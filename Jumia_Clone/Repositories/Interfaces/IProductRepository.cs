using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Models.DTOs.ProductAttributeValueDTOs;
using Jumia_Clone.Models.DTOs.ProductDTOs;
using Jumia_Clone.Models.DTOs.ProductImageDTOs;
using Jumia_Clone.Models.DTOs.ProductVariantDTOs2;

namespace Jumia_Clone.Repositories.Interfaces
{
    public interface IProductRepository
    {
        // Basic CRUD operations
        Task<ProductDto> GetProductByIdAsync(int id, bool includeDetails = false);
        Task<IEnumerable<ProductDto>> GetAllProductsAsync(PaginationDto pagination, ProductFilterDto filter = null);
        Task<ProductDto> CreateProductAsync(CreateProductInputDto productDto);
        Task<ProductDto> UpdateProductAsync(int id, UpdateProductInputDto productDto);
        Task DeleteProductAsync(int id);

        // Admin operations
        Task<ProductDto> UpdateProductApprovalStatusAsync(int id, string approvalStatus, string adminNotes = null);
        Task<IEnumerable<ProductDto>> GetPendingApprovalProductsAsync(PaginationDto pagination);

        // Seller operations
        Task<IEnumerable<ProductDto>> GetSellerProductsAsync(int sellerId, PaginationDto pagination, ProductFilterDto filter = null);

        // Variations
        Task<ProductVariantDto> AddProductVariantAsync(int productId, CreateProductVariantDto variantDto);
        Task<ProductVariantDto> UpdateProductVariantAsync(int variantId, UpdateProductVariantDto variantDto);
        Task DeleteProductVariantAsync(int variantId);

        // Images
        Task<ProductDto> UpdateProductMainImageAsync(int id, string imagePath);

        // Product attributes
        Task<ProductAttributeValueDto> AddProductAttributeValueAsync(int productId, CreateProductAttributeValueDto attributeValueDto);
        Task UpdateProductAttributeValueAsync(int valueId, UpdateProductAttributeValueDto attributeValueDto);

        // Inventory management
        Task<ProductDto> UpdateProductStockAsync(int id, int newStock);
        Task<ProductVariantDto> UpdateVariantStockAsync(int variantId, int newStock);

        // Analytics and recommendations
        Task<ProductStatisticsDto> GetProductStatisticsAsync(int productId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<ProductDto>> GetRelatedProductsAsync(int productId, int count = 5);
        Task<IEnumerable<ProductDto>> GetTrendingProductsAsync(int categoryId = 0, int subcategoryId = 0, int count = 10);
        Task<IEnumerable<ProductDto>> GetRandomProductsByCategoryAsync(string categoryName, int count = 15);
        Task<IEnumerable<ProductDto>> GetRandomProductsBySubcategoryAsync(string subcategoryName, int count = 15);



    }
}