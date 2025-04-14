using System.ComponentModel.DataAnnotations;

namespace Jumia_Clone.Models.DTOs.WishlistItemDTOs
{
    public class AddWishlistItemDto
    {
        [Required]
        public int ProductId { get; set; }
    }
}
