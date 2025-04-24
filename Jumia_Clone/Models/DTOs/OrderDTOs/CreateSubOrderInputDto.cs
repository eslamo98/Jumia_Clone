using System.ComponentModel.DataAnnotations;

namespace Jumia_Clone.Models.DTOs.OrderDTOs
{
    public class CreateSubOrderInputDto
    {
        [Required]
        public int SellerId { get; set; }

        [Required]
        public List<CreateOrderItemInputDto> OrderItems { get; set; }
        
        // This property will be calculated by the backend
        public decimal Subtotal { get; set; }
    }

}
