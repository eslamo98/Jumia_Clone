namespace Jumia_Clone.Models.DTOs.UserRecommendationDTOs
{
    public class UserRecommendationdto
    {
        public int UserRecommendationId { get; set; }

        public int CustomerId { get; set; }

        public int ProductId { get; set; }

        public string RecommendationType { get; set; }

        public decimal Score { get; set; }

        public DateTime? LastUpdated { get; set; }
    }
}
