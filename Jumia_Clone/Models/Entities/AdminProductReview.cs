namespace Jumia_Clone.Models.Entities
{
    public class AdminProductReview
    {
        public int ReviewId { get; set; }
        public int ProductId { get; set; }
        public int AdminId { get; set; }
        public string PreviousStatus { get; set; }
        public string NewStatus { get; set; }
        public string Notes { get; set; }
        public DateTime ReviewedAt { get; set; }

        public virtual Product Product { get; set; }
        public virtual Admin Admin { get; set; }
    }
}
