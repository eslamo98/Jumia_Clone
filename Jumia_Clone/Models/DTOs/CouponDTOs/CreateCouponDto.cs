using System.ComponentModel.DataAnnotations;

namespace Jumia_Clone.Models.DTOs.CouponDTOs
{
    public class CreateCouponDto
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Code { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [Range(0.01, 10000)]
        public decimal DiscountAmount { get; set; }

        [Range(0, 10000)]
        public decimal? MinimumPurchase { get; set; }

        [Required]
        [RegularExpression("^(Fixed|Percentage)$", ErrorMessage = "Discount type must be either 'Fixed' or 'Percentage'")]
        public string DiscountType { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool? IsActive { get; set; } = true;

        [Range(0, 10000)]
        public int? UsageLimit { get; set; }
    }
}
