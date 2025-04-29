using Jumia_Clone.Models.DTOs.AiChatBotDTOs;

namespace Jumia_Clone.Services.Interfaces
{
    public interface IOpenAIClient
    {
        Task<ChatCompletionResponse> CreateChatCompletionAsync(ChatCompletionRequest request);
    }
}
