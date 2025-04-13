using System.ComponentModel.DataAnnotations;

namespace Jumia_Clone.Models.DTOs.UserDTOs
{
    public class UserStatusUpdateDto
    {
        [Required]
        public bool IsActive { get; set; }
    }
}
