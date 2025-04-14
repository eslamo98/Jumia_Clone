using System.ComponentModel.DataAnnotations;

namespace Jumia_Clone.Models.DTOs.WishlistItemDTOs
{
    public class MoveToCartDto
    {
        [Required]
        public int WishlistItemId { get; set; }
    }
}
