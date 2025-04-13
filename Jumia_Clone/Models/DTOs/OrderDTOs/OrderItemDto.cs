namespace Jumia_Clone.Models.DTOs.OrderDTOs
{
    public class OrderItemDto
    {
        public int OrderItemId { get; set; }
        public int SuborderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; }
        public decimal TotalPrice { get; set; }
        public int? VariantId { get; set; }
    }
}
