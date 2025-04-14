using System.ComponentModel.DataAnnotations;

namespace Jumia_Clone.Models.DTOs.WishlistItemDTOs
{
    public class MoveMultipleToCartDto
    {
        [Required]
        public List<int> WishlistItemIds { get; set; }
    }
}
