using System.ComponentModel.DataAnnotations;

namespace Jumia_Clone.Models.DTOs.CouponDTOs
{
    public class ValidateCouponDto
    {
        [Required]
        public string CouponCode { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public decimal CartTotal { get; set; }
    }
}
