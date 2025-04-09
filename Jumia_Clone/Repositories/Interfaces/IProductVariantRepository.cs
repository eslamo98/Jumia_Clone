using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Models.DTOs.ProductVariantDTOs;

namespace Jumia_Clone.Repositories.Interfaces
{
    public interface IProductVariantRepository
    {
        // Get all variants for a product
        Task<IEnumerable<ProductVariantDto>> GetAllVariantsByProductIdAsync(int productId, PaginationDto pagination);

        // Get a specific variant by ID
        Task<ProductVariantDto> GetVariantByIdAsync(int productId, int variantId);

        // Add a new variant to a product
        Task<ProductVariantDto> CreateVariantAsync(int productId, CreateProductVariantDto variantDto);

        // Update an existing variant
        Task<ProductVariantDto> UpdateVariantAsync(int productId, UpdateProductVariantDto variantDto);

        // Delete a variant
        Task DeleteVariantAsync(int productId, int variantId);

        // Update product approval status (admin)
        //Task<object> UpdateProductApprovalAsync(int productId, ProductApprovalDto approvalDto);

        // Check if this is the only variant for the product
        Task<bool> IsLastVariantAsync(int productId, int variantId);

        // Set a new default variant if the current default is deleted
        Task SetNewDefaultVariantAsync(int productId, int excludeVariantId);
    }
}
