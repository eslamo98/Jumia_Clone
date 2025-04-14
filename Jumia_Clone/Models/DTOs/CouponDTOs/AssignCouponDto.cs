using System.ComponentModel.DataAnnotations;

namespace Jumia_Clone.Models.DTOs.CouponDTOs
{
    public class AssignCouponDto
    {
        [Required]
        public int CouponId { get; set; }

        [Required]
        public int CustomerId { get; set; }
    }
}
