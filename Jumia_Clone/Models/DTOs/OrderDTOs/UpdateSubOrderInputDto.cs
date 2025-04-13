using System.ComponentModel.DataAnnotations;

namespace Jumia_Clone.Models.DTOs.OrderDTOs
{
    public class UpdateSubOrderInputDto
    {
        [Required]
        public int SuborderId { get; set; }

        [StringLength(20)]
        public string Status { get; set; }

        [StringLength(100)]
        public string TrackingNumber { get; set; }

        [StringLength(100)]
        public string ShippingProvider { get; set; }
    }
}
