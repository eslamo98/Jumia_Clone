using System.ComponentModel.DataAnnotations;

namespace Jumia_Clone.Models.DTOs.AuthenticationDTOs
{
    public class RefreshTokenDto
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}
