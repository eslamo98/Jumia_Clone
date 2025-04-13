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
        [Range(0.01, double.MaxValue, ErrorMessage = "Total amount must be greater than 0")]
        public decimal TotalAmount { get; set; }

        public decimal? DiscountAmount { get; set; }

        public decimal? ShippingFee { get; set; }

        public decimal? TaxAmount { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Final amount must be greater than 0")]
        public decimal FinalAmount { get; set; }

        [Required]
        [StringLength(20)]
        public string PaymentMethod { get; set; }

        public int? AffiliateId { get; set; }

        [StringLength(20)]
        public string AffiliateCode { get; set; }

        [Required]
        public List<CreateSubOrderInputDto> SubOrders { get; set; }
    }
}
