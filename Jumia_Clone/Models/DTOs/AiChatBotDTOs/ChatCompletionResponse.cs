using System.Text.Json.Serialization;

namespace Jumia_Clone.Models.DTOs.AiChatBotDTOs
{
    public class ChatCompletionResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("object")]
        public string Object { get; set; }

        [JsonPropertyName("created")]
        public long Created { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("choices")]
        public List<Choice> Choices { get; set; }

        [JsonPropertyName("usage")]
        public Usage Usage { get; set; }

        [JsonPropertyName("system_fingerprint")]
        public string SystemFingerprint { get; set; }
    }

    public class Choice
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("message")]
        public ResponseMessage Message { get; set; }

        [JsonPropertyName("logprobs")]
        public object LogProbs { get; set; }

        [JsonPropertyName("finish_reason")]
        public string FinishReason { get; set; }
    }

    public class ResponseMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("refusal")]
        public object Refusal { get; set; }

        [JsonPropertyName("annotations")]
        public List<object> Annotations { get; set; }
    }

    public class Usage
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }

        [JsonPropertyName("prompt_tokens_details")]
        public TokenDetails PromptTokensDetails { get; set; }

        [JsonPropertyName("completion_tokens_details")]
        public CompletionTokenDetails CompletionTokensDetails { get; set; }
    }

    public class TokenDetails
    {
        [JsonPropertyName("cached_tokens")]
        public int CachedTokens { get; set; }

        [JsonPropertyName("audio_tokens")]
        public int AudioTokens { get; set; }
    }

    public class CompletionTokenDetails : TokenDetails
    {
        [JsonPropertyName("reasoning_tokens")]
        public int ReasoningTokens { get; set; }

        [JsonPropertyName("accepted_prediction_tokens")]
        public int AcceptedPredictionTokens { get; set; }

        [JsonPropertyName("rejected_prediction_tokens")]
        public int RejectedPredictionTokens { get; set; }
    }
}
