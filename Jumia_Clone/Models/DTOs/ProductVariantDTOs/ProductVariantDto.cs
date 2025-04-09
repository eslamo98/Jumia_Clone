using Jumia_Clone.Models.DTOs.VariantAttributeDTOs;

namespace Jumia_Clone.Models.DTOs.ProductVariantDTOs
{
    public class ProductVariantDto
    {
        public int VariantId { get; set; }
        public int ProductId { get; set; }
        public string VariantName { get; set; }
        public decimal Price { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal FinalPrice { get; set; }
        public int StockQuantity { get; set; }
        public string Sku { get; set; }
        public string VariantImageUrl { get; set; }
        public bool IsDefault { get; set; }
        public bool IsAvailable { get; set; }
        public List<VariantAttributeDto> Attributes { get; set; }
    }
}
