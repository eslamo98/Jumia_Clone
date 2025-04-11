namespace Jumia_Clone.Models.DTOs.CouponDTOs    
{
    public class Coupondto
    {
        public int CouponId { get; set; }

        public string Code { get; set; }

        public string Description { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal? MinimumPurchase { get; set; }

        public string DiscountType { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool? IsActive { get; set; }

        public int? UsageLimit { get; set; }

        public int? UsageCount { get; set; }
    }
}
