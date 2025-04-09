namespace Jumia_Clone.Models.DTOs.CartItemDtos
{
    public class AddItemToCartDto
    {
        public int ProductId { get; set; }
        public int? VariantId { get; set; }  // Optional
        public int Quantity { get; set; }
    }
}
