namespace Jumia_Clone.Models.DTOs.ProductVariantDTOs
{
    public class ProductVariantDto
    {
        public int VariantId { get; set; }
        public string VariantName { get; set; }
        public decimal Price { get; set; }
        public decimal DiscountPercentage { get; set; }
        public int StockQuantity { get; set; }
        public string SKU { get; set; }
        public string VariantImageUrl { get; set; }
        public bool IsDefault { get; set; }
        public List<VariantAttributeDto> Attributes { get; set; }
    }
}
