namespace Jumia_Clone.Models.DTOs.ProductVariantDTOs
{
    public class VariantFilterDto
    {
        public int? ProductId { get; set; }
        public string SearchTerm { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? InStock { get; set; }
        public bool? IsAvailable { get; set; }
        public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
    }
}
