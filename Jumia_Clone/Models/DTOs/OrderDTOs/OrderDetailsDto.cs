namespace Jumia_Clone.Models.DTOs.OrderDTOs
{
    public class OrderDetailsDto
    {
        public int OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public List<OrderItemDetailsDto> Items { get; set; }
        public List<SubOrderDto> SubOrders { get; set; }  // Add this line
    }

    public class OrderItemDetailsDto
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; }
    }
}
