
using Jumia_Clone.Models.DTOs.ProductDTOs;

namespace Jumia_Clone.Models.DTOs.ProductVariantDTOs2
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
        public List<ProductVariantAttributeDto> Attributes { get; set; }
    }
}
