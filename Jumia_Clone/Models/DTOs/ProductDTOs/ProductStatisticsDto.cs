namespace Jumia_Clone.Models.DTOs.ProductDTOs
{
    public class ProductStatisticsDto
    {
        public int ProductId { get; set; }
        public int TotalViews { get; set; }
        public int UniqueViews { get; set; }
        public int TotalSales { get; set; }
        public decimal TotalRevenue { get; set; }
        public double ConversionRate { get; set; }
        public double AverageRating { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
