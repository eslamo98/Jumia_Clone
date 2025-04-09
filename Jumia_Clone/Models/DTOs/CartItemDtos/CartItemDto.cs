namespace Jumia_Clone.Models.DTOs.CartItemDtos
{
    public class CartItemDto
    {
        public int CartItemId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }
    }
}
