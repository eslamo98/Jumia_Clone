namespace Jumia_Clone.Models.DTOs.WishlistItemDTOs
{
    public class WishlistDto
    {
        public int WishlistId { get; set; }
        public int CustomerId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string CustomerName { get; set; }
        public int ItemsCount { get; set; }
        public List<WishlistItemDto> WishlistItems { get; set; } = new List<WishlistItemDto>();
    }
}
