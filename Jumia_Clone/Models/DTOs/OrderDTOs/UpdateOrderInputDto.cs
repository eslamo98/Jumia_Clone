using System.ComponentModel.DataAnnotations;

namespace Jumia_Clone.Models.DTOs.OrderDTOs
{
    // DTO for updating an order
    public class UpdateOrderInputDto
    {
        [Required]
        public int OrderId { get; set; }

        public int? CouponId { get; set; }

        public decimal? DiscountAmount { get; set; }

        public decimal? ShippingFee { get; set; }

        public decimal? TaxAmount { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Final amount must be greater than 0")]
        public decimal? FinalAmount { get; set; }

        [StringLength(10)]
        public string PaymentStatus { get; set; }
    }
}
