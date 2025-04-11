namespace Jumia_Clone.Models.DTOs.UserDTOs
{
    public class Userdto
    {
        public int UserId { get; set; }

        public string Email { get; set; }

        public string PasswordHash { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string PhoneNumber { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string UserType { get; set; }

        public bool? IsActive { get; set; }
    }
}
