using Jumia_Clone.Data;
using Jumia_Clone.Models.DTOs.AiChatBotDTOs;
using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Models.Enums;
using Jumia_Clone.Services.Implementation;
using Jumia_Clone.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text.Json;

namespace Jumia_Clone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AIQueryController : ControllerBase
    {
        private readonly IAIQueryService _aiQueryService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AIQueryController> _logger;
        private readonly IOpenAIClient _openAIClient;

        public AIQueryController(
            IAIQueryService aiQueryService,
            ApplicationDbContext context,
            ILogger<AIQueryController> logger,
            IOpenAIClient openAIClient)
        {
            _aiQueryService = aiQueryService;
            _context = context;
            _logger = logger;
            _openAIClient = openAIClient;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> AskQuestion([FromBody] string question)
        {
            try
            {
                // Step 1: Generate SQL query from user question
                var generatedQuery = await _aiQueryService.GenerateQueryFromUserQuestion(question);

                // Step 2: Validate the generated query
                var validationResult = _aiQueryService.ValidateGeneratedQuery(generatedQuery);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        Message = "Invalid query generated",
                        ErrorMessages = new[] { validationResult.Message }
                    });
                }

                // Step 3: Execute the validated query
                var queryResults = await _context.Database
                    .CreateExecutionStrategy()
                    .ExecuteAsync(async () =>
                    {
                        using var command = _context.Database.GetDbConnection().CreateCommand();
                        command.CommandText = validationResult.SafeQuery;
                        command.CommandType = CommandType.Text;

                        var results = new List<Dictionary<string, object>>();
                        await _context.Database.OpenConnectionAsync();

                        using var reader = await command.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            var row = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row[reader.GetName(i)] = reader.GetValue(i);
                            }
                            results.Add(row);
                        }

                        return results;
                    });

                // Step 4: Generate human-readable response
                var messages = new List<ChatMessage>
                {
                    new ChatMessage() { Role = ChatRole.System, Content = "You are a helpful e-commerce assistant. Format the query results into a natural, friendly response." },
                    new ChatMessage() { Role = ChatRole.User, Content = $"Question: {question}\nResults: {JsonSerializer.Serialize(queryResults)}" }
                };
                var chatRequest = new ChatCompletionRequest
                {
                    Messages = messages,
                    Model = "gpt-4o-mini",
                    Temperature = 0.7,
                    MaxTokens = 500
                };

                var response = await _openAIClient.CreateChatCompletionAsync(chatRequest);
                var finalResponse = response.Choices[0].Message.Content;

                return Ok(new ApiResponse<object>
                {
                    Message = "Query processed successfully",
                    Data = new
                    {
                        Question = question,
                        GeneratedQuery = generatedQuery,
                        RawResults = queryResults,
                        FormattedResponse = finalResponse
                    },
                    Success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing AI query");
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "Error processing query",
                    ErrorMessages = new[] { ex.Message }
                });
            }
        }
    }
}
