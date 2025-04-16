namespace Jumia_Clone.Models.DTOs.ProductVariantDTOs
{
    public class ProductVariantSummaryDto
    {
        public int VariantId { get; set; }
        public string VariantName { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public decimal DiscountedPrice => Price - (Price * (DiscountPercentage ?? 0) / 100);
        public int StockQuantity { get; set; }
        public bool IsAvailable { get; set; }
        public string VariantImageUrl { get; set; }
        public string AttributeSummary { get; set; } // e.g., "Color: Red, Size: XL"
    }
}
