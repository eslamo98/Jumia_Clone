namespace Jumia_Clone.Models.DTOs.RatingDTOs
{
    public class Ratingdto
    {
        public int RatingId { get; set; }

        public int CustomerId { get; set; }

        public int ProductId { get; set; }

        public int Stars { get; set; }

        public string Comment { get; set; }

        public DateTime? CreatedAt { get; set; }

        public bool? IsVerifiedPurchase { get; set; }

        public int? HelpfulCount { get; set; }
    }
}
