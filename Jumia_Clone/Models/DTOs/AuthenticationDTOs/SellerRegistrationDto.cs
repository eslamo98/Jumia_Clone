using System.ComponentModel.DataAnnotations;

namespace Jumia_Clone.Models.DTOs.AuthenticationDTOs
{
    public class SellerRegistrationDto
    {
        [Required]
        public string BusinessName { get; set; }

        public string BusinessDescription { get; set; }

        public string BusinessLogo { get; set; }
    }
}
