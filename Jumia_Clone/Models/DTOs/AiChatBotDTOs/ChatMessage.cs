using Jumia_Clone.Models.Enums;

namespace Jumia_Clone.Models.DTOs.AiChatBotDTOs
{
    public class ChatMessage
    {
        public ChatMessage() { }
        public ChatMessage(ChatRole role, string content)
        {
            Role = role;
            Content = content;
        }

        public ChatRole Role { get; set; }
        public string Content { get; set; }
    }
}
