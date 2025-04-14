namespace Jumia_Clone.Models.DTOs.CartDTOs
{
    public class CartItemDto
    {
        public int CartItemId { get; set; }
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAtAddition { get; set; }
        public decimal TotalPrice { get => PriceAtAddition * Quantity; }
        public int? VariantId { get; set; }
        public string VariantName { get; set; }
    }
}
