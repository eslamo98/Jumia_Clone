namespace Jumia_Clone.Models.DTOs.SearchHistoryDTOs
{
    public class SearchHistorydto
    {
        public int SearchId { get; set; }

        public int? CustomerId { get; set; }

        public string SessionId { get; set; }

        public string SearchQuery { get; set; }

        public DateTime? SearchTime { get; set; }

        public int? ResultCount { get; set; }

        public int? CategoryId { get; set; }

        public int? SubcategoryId { get; set; }

        public string Filters { get; set; }
    }
}
