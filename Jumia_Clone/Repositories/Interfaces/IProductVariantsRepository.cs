using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Models.DTOs.ProductVariantDTOs;
using Jumia_Clone.Models.Entities;

namespace Jumia_Clone.Repositories.Interfaces
{
    public interface IProductVariantsRepository
    {
        // Basic CRUD operations
        Task<ProductVariant> GetByIdAsync(int variantId);
        Task<ProductVariant> GetByIdWithDetailsAsync(int variantId);
        Task<IEnumerable<ProductVariant>> GetAllAsync(int? productId = null);

        // Create variant (note: image handling is done in service/controller)
        Task<ProductVariant> AddAsync(ProductVariant variant);

        // Update variant (note: image handling is done in service/controller)
        Task<ProductVariant> UpdateAsync(ProductVariant variant, List<UpdateVariantAttributeDto> attributes = null);

        // Update variant image path
        Task<ProductVariant> UpdateVariantImageAsync(int variantId, string imagePath);

        // Delete variant
        Task<bool> DeleteAsync(int variantId);

        // Stock management
        Task<ProductVariant> UpdateStockAsync(int variantId, int newStock);
        Task<bool> DecrementStockAsync(int variantId, int quantity);
        Task<IEnumerable<ProductVariant>> GetLowStockVariantsAsync(int threshold = 10);

        // Default variant management
        Task<ProductVariant> GetDefaultVariantForProductAsync(int productId);
        Task<ProductVariant> SetAsDefaultVariantAsync(int variantId);

        // Attribute management
        Task<IEnumerable<VariantAttribute>> GetVariantAttributesAsync(int variantId);
        Task<VariantAttribute> AddVariantAttributeAsync(VariantAttribute attribute);
        Task<VariantAttribute> UpdateVariantAttributeAsync(VariantAttribute attribute);
        Task<bool> DeleteVariantAttributeAsync(int attributeId);

        // Search and filtering
        Task<IEnumerable<ProductVariant>> GetVariantsByAttributesAsync(int productId, Dictionary<string, string> attributeFilters);
        Task<IEnumerable<ProductVariant>> GetVariantsBySKUAsync(string sku);
        Task<IEnumerable<ProductVariant>> GetVariantsByPriceRangeAsync(decimal minPrice, decimal maxPrice);

        // Helper methods
        Task<bool> VariantExistsAsync(int variantId);
        Task<bool> VariantNameExistsForProductAsync(int productId, string variantName, int? excludeVariantId = null);
        Task<ProductVariant> UpsertVariantAsync(ProductVariant variant, int productId);
        public Task SyncVariantAttributesAsync(int variantId, IEnumerable<UpdateVariantAttributeDto> newAttributes);

    }
}
