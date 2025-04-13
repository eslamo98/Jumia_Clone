namespace Jumia_Clone.Models.DTOs.TrendingProductDTOs
{
    public class TrendingProductdto
    {
        public int TrendingId { get; set; }

        public int ProductId { get; set; }

        public int? CategoryId { get; set; }

        public int? SubcategoryId { get; set; }

        public decimal TrendScore { get; set; }

        public string TimePeriod { get; set; }

        public DateTime? LastUpdated { get; set; }
    }
}
