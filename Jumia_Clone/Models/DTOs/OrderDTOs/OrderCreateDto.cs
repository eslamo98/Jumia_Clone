namespace Jumia_Clone.Models.DTOs.OrderDTOs
{
    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; }
        public int? VariantId { get; set; }
    }

    public class SubOrderDto
    {
        public int SellerId { get; set; }
        public List<OrderItemDto> OrderItems { get; set; }
    }



    public class CreateOrderDto
    {
        public int CustomerId { get; set; }
        public int AddressId { get; set; }
        public int? CouponId { get; set; }
        public string PaymentMethod { get; set; }
        public List<OrderItemDto> Items { get; set; }
    }

}
