namespace Jumia_Clone.Models.DTOs.CartItemDtos
{
    public class AddItemToCartDto
    {
        public int ProductId { get; set; }
        public int? VariantId { get; set; }  
        public int Quantity { get; set; }

        public decimal PriceAtAddition { get; set; }
    }
}
