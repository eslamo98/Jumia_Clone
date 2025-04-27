using Jumia_Clone.Models.DTOs.ProductDTOs;

namespace Jumia_Clone.Models.DTOs.ProductVariantDTOs2
{
    public class CreateProductBaseVariantDto
    {
        public int? VariantId { get; set; }
        public string VariantName { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public int StockQuantity { get; set; }
        public string Sku { get; set; }
        public bool IsDefault { get; set; }
        public string VariantImageBase64 { get; set; }
        public List<ProductVariantAttributeDto> VariantAttributes { get; set; }
    }
}
