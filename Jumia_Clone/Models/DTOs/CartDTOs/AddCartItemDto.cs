using System.ComponentModel.DataAnnotations;

namespace Jumia_Clone.Models.DTOs.CartDTOs
{
    public class AddCartItemDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
        public int Quantity { get; set; }

        public int? VariantId { get; set; }
    }
}
