using System.ComponentModel.DataAnnotations;

namespace Jumia_Clone.Models.DTOs.OrderDTOs
{
    public class UpdateOrderStatusDto
    {
        [Required]
        [StringLength(20)]
        public string Status { get; set; }
    }
}
