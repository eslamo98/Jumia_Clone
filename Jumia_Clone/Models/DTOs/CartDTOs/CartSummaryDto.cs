namespace Jumia_Clone.Models.DTOs.CartDTOs
{
    public class CartSummaryDto
    {
        public int CartId { get; set; }
        public int ItemsCount { get; set; }
        public decimal SubTotal { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}
