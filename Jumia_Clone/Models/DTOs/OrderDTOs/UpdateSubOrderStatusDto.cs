using System.ComponentModel.DataAnnotations;

namespace Jumia_Clone.Models.DTOs.OrderDTOs
{
    public class UpdateSubOrderStatusDto
    {
        [Required]
        [StringLength(20)]
        public string Status { get; set; }
    }
}
