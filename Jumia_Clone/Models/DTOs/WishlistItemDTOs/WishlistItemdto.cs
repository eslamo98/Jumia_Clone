namespace Jumia_Clone.Models.DTOs.WishlistItemDTOs
{
    public class WishlistItemdto
    {
        public int WishlistItemId { get; set; }

        public int WishlistId { get; set; }

        public int ProductId { get; set; }

        public DateTime? AddedAt { get; set; }
    }
}
