namespace Jumia_Clone.Models.DTOs.AdminDTOs
{
    public class AdminProductReviewdto
    {
        public int AdminProductReviewId { get; set; }
        public int ProductId { get; set; }
        public int AdminId { get; set; }
        public string PreviousStatus { get; set; }
        public string NewStatus { get; set; }
        public string Notes { get; set; }
        public DateTime ReviewedAt { get; set; }
    }
}
