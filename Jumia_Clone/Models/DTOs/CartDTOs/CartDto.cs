using Jumia_Clone.Models.DTOs.CartItemDtos;

namespace Jumia_Clone.Models.DTOs.CartDTOs
{
    public class CartDto
    {
        public int CartId { get; set; }
        public List<CartItemDto> Items { get; set; }
        public CartSummaryDto Summary { get; set; }
    }
}
