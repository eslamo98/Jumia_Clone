namespace Jumia_Clone.Models.DTOs.AiChatBotDTOs
{
    public class QueryValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public string SafeQuery { get; set; }
    }
}
