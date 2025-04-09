namespace Jumia_Clone.Models.DTOs.AuthenticationDTOs
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string FullName { get; set; }
        public string UserType { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}
