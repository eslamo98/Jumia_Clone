using Jumia_Clone.Data;
using Jumia_Clone.Models.DTOs.AiChatBotDTOs;
using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Models.Enums;
using System.Drawing;
using System.Drawing.Imaging;
using Jumia_Clone.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using Microsoft.ML.Vision;
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


        [HttpPost("search-by-image")]
        public async Task<IActionResult> SearchByImage(IFormFile image)
        {
            try
            {
                if (image == null || image.Length == 0)
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        Message = "No image uploaded",
                        ErrorMessages = new[] { "Please provide a valid image file" }
                    });
                }

                // Convert image to base64
                using var memoryStream = new MemoryStream();
                await image.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();
                var base64Image = Convert.ToBase64String(imageBytes);

                // Get image description using OpenAI vision model
                var messages = new List<ChatMessage>
        {
            new ChatMessage
            {
                Role = ChatRole.System,
                Content = "You are a helpful assistant that analyzes product images. Focus on identifying the product type, brand, and model. Be specific and concise."
            },
            new ChatMessage
            {
                Role = ChatRole.User,
                Content = "What product is shown in this image? Provide a brief, specific description focusing on the product type, brand, and model if visible.",
                ImageBase64 = $"data:image/{Path.GetExtension(image.FileName).TrimStart('.')};base64,{base64Image}"
            }
        };

                var chatRequest = new ChatCompletionRequest
                {
                    Messages = messages,
                    Model = "gpt-4o-mini",
                    Temperature = 0.3,
                    MaxTokens = 500
                };

                var response = await _openAIClient.CreateChatCompletionAsync(chatRequest);
                var productDescription = response.Choices[0].Message.Content;

                // Generate search query using the service
                var searchQuery = await _aiQueryService.GenerateQueryFromUserQuestion($"Find products similar to: {productDescription}");
                var validationResult = _aiQueryService.ValidateGeneratedQuery(searchQuery);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        Message = "Invalid query generated",
                        ErrorMessages = new[] { validationResult.Message }
                    });
                }

                // Execute the database query
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

                return Ok(new ApiResponse<object>
                {
                    Message = "Image search completed successfully",
                    Data = new
                    {
                        Description = productDescription,
                        GeneratedQuery = searchQuery,
                        Results = queryResults
                    },
                    Success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing image search");
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "Error processing image search",
                    ErrorMessages = new[] { ex.Message }
                });
            }
        }
        public class ImageData
        {
            public string Image { get; set; }  // Changed from byte[] to string to store file path
        }
    }

}