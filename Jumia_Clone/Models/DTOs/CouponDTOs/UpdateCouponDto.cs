using System.ComponentModel.DataAnnotations;

namespace Jumia_Clone.Models.DTOs.CouponDTOs
{
    public class UpdateCouponDto
    {
        [Required]
        public int CouponId { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Range(0.01, 10000)]
        public decimal? DiscountAmount { get; set; }

        [Range(0, 10000)]
        public decimal? MinimumPurchase { get; set; }

        [RegularExpression("^(Fixed|Percentage)$", ErrorMessage = "Discount type must be either 'Fixed' or 'Percentage'")]
        public string DiscountType { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool? IsActive { get; set; }

        [Range(0, 10000)]
        public int? UsageLimit { get; set; }
    }
}
