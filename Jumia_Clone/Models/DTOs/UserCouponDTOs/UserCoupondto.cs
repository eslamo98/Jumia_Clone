namespace Jumia_Clone.Models.DTOs.UserCouponDTOs
{
    public class UserCoupondto
    {
        public int UserCouponId { get; set; }

        public int CustomerId { get; set; }

        public int CouponId { get; set; }

        public bool? IsUsed { get; set; }

        public DateTime? AssignedAt { get; set; }

        public DateTime? UsedAt { get; set; }

    }
}
