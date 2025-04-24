using System.ComponentModel.DataAnnotations;

namespace Jumia_Clone.Models.DTOs.OrderDTOs
{
    public class CreateOrderItemInputDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        public int? VariantId { get; set; }
        
        // These properties will be calculated by the backend
        public decimal PriceAtPurchase { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
