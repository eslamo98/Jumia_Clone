namespace Jumia_Clone.Models.DTOs.WishlistItemDTOs
{
    public class WishlistSummaryDto
    {
        public int WishlistId { get; set; }
        public int CustomerId { get; set; }
        public int ItemsCount { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}
