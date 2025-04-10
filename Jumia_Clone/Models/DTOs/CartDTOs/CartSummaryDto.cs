namespace Jumia_Clone.Models.DTOs.CartDTOs
{
    public class CartSummaryDto
    {
        public decimal Subtotal { get; set; }
        public int TotalItems { get; set; }
        public int SellerCount { get; set; }
    }
}
