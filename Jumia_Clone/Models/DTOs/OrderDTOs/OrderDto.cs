namespace Jumia_Clone.Models.DTOs.OrderDTOs
{
    public class OrderDto
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int AddressId { get; set; }
        public string Address { get; set; }
        public int? CouponId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? ShippingFee { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? AffiliateId { get; set; }
        public string AffiliateCode { get; set; }
        public string OrderStatus { get; set; }
        public List<SubOrderDto> SubOrders { get; set; }
    }

}
