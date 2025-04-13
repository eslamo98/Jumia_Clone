using System.ComponentModel.DataAnnotations;

namespace Jumia_Clone.Models.DTOs.UserDTOs
{
    public class AdminUpdateDto : UserUpdateDto
    {
        [StringLength(50)]
        public string Role { get; set; }

        public string Permissions { get; set; }
    }
}
