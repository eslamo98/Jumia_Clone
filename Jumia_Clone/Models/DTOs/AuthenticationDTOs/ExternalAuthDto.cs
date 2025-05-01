namespace Jumia_Clone.Models.DTOs.AuthenticationDTOs
{
    public class ExternalAuthDto
    {
        public string Provider { get; set; }  // "GOOGLE" or "FACEBOOK"
        public string IdToken { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string PhotoUrl { get; set; }
        public bool IsNewUser { get; set; }  
    }
}
