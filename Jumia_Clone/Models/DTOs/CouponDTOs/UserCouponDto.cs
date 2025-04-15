namespace Jumia_Clone.Models.DTOs.CouponDTOs
{
    public class UserCouponDto
    {
        public int UserCouponId { get; set; }
        public int CustomerId { get; set; }
        public int CouponId { get; set; }
        public bool? IsUsed { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? UsedAt { get; set; }
        public CouponDto Coupon { get; set; }
    }
}
