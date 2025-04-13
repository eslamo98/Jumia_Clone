using System.ComponentModel.DataAnnotations;

namespace Jumia_Clone.Models.DTOs.OrderDTOs
{
    public class CreateSubOrderInputDto
    {
        [Required]
        public int SellerId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Subtotal must be greater than 0")]
        public decimal Subtotal { get; set; }

        [Required]
        public List<CreateOrderItemInputDto> OrderItems { get; set; }
    }

}
