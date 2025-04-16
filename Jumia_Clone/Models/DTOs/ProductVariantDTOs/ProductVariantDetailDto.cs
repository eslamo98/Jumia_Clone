namespace Jumia_Clone.Models.DTOs.ProductVariantDTOs
{
    public class ProductVariantDetailDto
    {
        public int VariantId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string VariantName { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public decimal DiscountedPrice => Price - (Price * (DiscountPercentage ?? 0) / 100);
        public int StockQuantity { get; set; }
        public string Sku { get; set; }
        public string VariantImageUrl { get; set; }
        public bool? IsDefault { get; set; }
        public bool? IsAvailable { get; set; }
        public List<UpdateVariantAttributeDto> VariantAttributes { get; set; } = new List<UpdateVariantAttributeDto>();
    }
}
