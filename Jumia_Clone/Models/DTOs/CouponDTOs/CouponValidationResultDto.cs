namespace Jumia_Clone.Models.DTOs.CouponDTOs
{
    public class CouponValidationResultDto
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public CouponDto Coupon { get; set; }
        public decimal DiscountValue { get; set; }
    }
}
