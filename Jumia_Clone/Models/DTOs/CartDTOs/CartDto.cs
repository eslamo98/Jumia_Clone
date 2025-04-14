
namespace Jumia_Clone.Models.DTOs.CartDTOs
{
    public class CartDto
    {
        public int CartId { get; set; }
        public int CustomerId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<CartItemDto> CartItems { get; set; } = new List<CartItemDto>();
    }
}
