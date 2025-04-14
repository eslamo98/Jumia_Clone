namespace Jumia_Clone.Models.DTOs.UserDTOs
{
    public class AdminDto : UserDto
    {
        public int AdminId { get; set; }
        public string Role { get; set; }
        public string Permissions { get; set; }
    }
}
