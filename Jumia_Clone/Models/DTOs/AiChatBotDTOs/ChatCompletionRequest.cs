namespace Jumia_Clone.Models.DTOs.AiChatBotDTOs
{
    public class ChatCompletionRequest
    {
        public List<ChatMessage> Messages { get; set; }
        public string Model { get; set; }
        public double Temperature { get; set; }
        public int MaxTokens { get; set; }
    }
}
