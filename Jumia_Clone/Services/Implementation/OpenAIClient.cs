using Jumia_Clone.Models.DTOs.AiChatBotDTOs;
using Jumia_Clone.Services.Interfaces;
using System.Net.Http.Headers;

namespace Jumia_Clone.Services.Implementation
{
    public class OpenAIClient : IOpenAIClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public OpenAIClient(IConfiguration configuration)
        {
            _apiKey = configuration["OpenAI:ApiKey"];
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.openai.com/v1/")
            };
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        }

       

        public async Task<ChatCompletionResponse> CreateChatCompletionAsync(ChatCompletionRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("chat/completions", new
            {
                model = request.Model,
                messages = request.Messages.Select(m => new { role = m.Role.ToString().ToLower(), content = m.Content }),
                temperature = request.Temperature,
                max_tokens = request.MaxTokens
            });

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<ChatCompletionResponse>();
        }


    }
}
