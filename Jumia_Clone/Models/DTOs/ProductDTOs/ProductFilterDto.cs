namespace Jumia_Clone.Models.DTOs.ProductDTOs
{
    public class ProductFilterDto
    {
        public int? CategoryId { get; set; }
        public int? SubcategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string SearchTerm { get; set; }
        public string ApprovalStatus { get; set; }
        public string SortBy { get; set; }
        public string SortDirection { get; set; }
    }
}
