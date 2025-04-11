using Jumia_Clone.Models.Entities;

namespace Jumia_Clone.Models.DTOs.SearchResultClickDTOs
{
    public class SearchResultClickdto
    {
        public int ClickId { get; set; }

        public int SearchId { get; set; }

        public int ProductId { get; set; }

        public DateTime? ClickTime { get; set; }

        public int? PositionInResults { get; set; }

    
    }
}
