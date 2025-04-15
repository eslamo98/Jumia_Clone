namespace Jumia_Clone.Models.DTOs.WishlistItemDTOs
{
    public class WishlistItemDto
    {
        public int WishlistItemId { get; set; }
        public int WishlistId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public decimal BasePrice { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public decimal CurrentPrice { get; set; }
        public string MainImageUrl { get; set; }
        public bool IsAvailable { get; set; }
        public int? StockQuantity { get; set; }
        public DateTime? AddedAt { get; set; }
    }
}
