namespace Jumia_Clone.Models.DTOs.AdminDTOs
{
    public class Admindto
    {
        public int AdminId { get; set; }

        public int UserId { get; set; }

        public string Role { get; set; }

        public string Permissions { get; set; }
    }
}
