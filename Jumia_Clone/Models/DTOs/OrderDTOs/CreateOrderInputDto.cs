using System.ComponentModel.DataAnnotations;

namespace Jumia_Clone.Models.DTOs.OrderDTOs
{
    public class CreateOrderInputDto
    {
        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int AddressId { get; set; }

        public int? CouponId { get; set; }

        [Required]
        [StringLength(20)]
        public string PaymentMethod { get; set; }

        public int? AffiliateId { get; set; }

        [StringLength(20)]
        public string AffiliateCode { get; set; }

        [Required]
        public List<CreateOrderItemInputDto> OrderItems { get; set; }
        
        // These properties will be calculated by the backend
        public decimal TotalAmount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? ShippingFee { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal FinalAmount { get; set; }
        
        // This will be populated by the backend
        public List<CreateSubOrderInputDto> SubOrders { get; set; }
    }
}
