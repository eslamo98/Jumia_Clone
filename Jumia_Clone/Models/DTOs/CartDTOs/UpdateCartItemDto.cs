using System.ComponentModel.DataAnnotations;

namespace Jumia_Clone.Models.DTOs.CartDTOs
{
    public class UpdateCartItemDto
    {
        [Required]
        public int CartItemId { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
        public int Quantity { get; set; }
    }
}
