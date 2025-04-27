namespace Jumia_Clone.Models.DTOs.AuthenticationDTOs
{
    public class UserResponseDto
    {
        public int UserId { get; set; }
        public int EntityId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserType { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}
