namespace Jumia_Clone.Models.DTOs.OrderDTOs
{
    public class SubOrderDto
    {
        public int SuborderId { get; set; }
        public int OrderId { get; set; }
        public int SellerId { get; set; }
        public string SellerName { get; set; }
        public decimal Subtotal { get; set; }
        public string Status { get; set; }
        public DateTime? StatusUpdatedAt { get; set; }
        public string TrackingNumber { get; set; }
        public string ShippingProvider { get; set; }
        public List<OrderItemDto> OrderItems { get; set; }
    }
}
