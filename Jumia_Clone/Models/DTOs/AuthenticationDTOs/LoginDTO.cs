using System.ComponentModel.DataAnnotations;

namespace Jumia_Clone.Models.DTOs.AuthenticationDTOs
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
