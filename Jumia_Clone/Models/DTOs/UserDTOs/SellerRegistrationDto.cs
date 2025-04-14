using System.ComponentModel.DataAnnotations;

namespace Jumia_Clone.Models.DTOs.UserDTOs
{
    public class SellerRegistrationDto : UserRegistrationDto
    {
        [Required]
        [StringLength(255)]
        public string BusinessName { get; set; }

        public string BusinessDescription { get; set; }

        public IFormFile BusinessLogo { get; set; }
    }
}
