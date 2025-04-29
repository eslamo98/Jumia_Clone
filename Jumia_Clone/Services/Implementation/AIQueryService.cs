using Jumia_Clone.Data;
using Jumia_Clone.Models.DTOs.AiChatBotDTOs;
using Jumia_Clone.Models.Enums;
using Jumia_Clone.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Jumia_Clone.Services.Implementation
{
    public class AIQueryService : IAIQueryService
    {
        private readonly IOpenAIClient _openAIClient;
        private readonly ILogger<AIQueryService> _logger;
        private readonly ApplicationDbContext _context;

        public AIQueryService(IOpenAIClient openAIClient, ILogger<AIQueryService> logger, ApplicationDbContext context)
        {
            _openAIClient = openAIClient;
            _logger = logger;
            _context = context;
        }

        public async Task<string> GenerateQueryFromUserQuestion(string userQuestion)
        {
            try
            {
                var schema = GetDatabaseSchema();
                var messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System, GetSystemPrompt()),
                new ChatMessage(ChatRole.User, $"Schema:\n{schema}\n\nQuestion: {userQuestion}")
            };

                var chatRequest = new ChatCompletionRequest
                {
                    Messages = messages,
                    Model = "gpt-4o-mini",
                    Temperature = 0.2,
                    MaxTokens = 500
                };

                var response = await _openAIClient.CreateChatCompletionAsync(chatRequest);
                return response.Choices[0].Message.Content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating query from user question");
                throw;
            }
        }

        public string GetSystemPrompt()
        {
            return @"You are a SQL query generator for an e-commerce platform.
            Here is the database description:
            This is an e-commerce database for a Jumia clone with the following key components:

            User Management System:
            Users can have three roles: Admin, Customer, and Seller
            Each user has basic information and can have multiple addresses
            Customers can maintain carts, wishlists, and order history
            Sellers can manage their products and track sales
            Admins have special permissions for platform management
            Product Hierarchy:
            Categories are the top level (e.g., Electronics, Fashion)
            Each Category contains multiple SubCategories
            SubCategories define specific product types and their attributes
            Products belong to SubCategories and inherit their attribute definitions
            Each Product can have multiple variants with different attribute values
            Product Attribute System:
            SubCategories define custom attributes (e.g., size, color for clothing)
            Products have values for these attributes in ProductAttributeValues
            Product variants can have their own specific attribute values
            This creates a flexible system for handling different product types
            Product Variants:
            Products can have multiple variants (e.g., different sizes/colors)
            Each variant can have its own:
            Price and stock quantity
            Specific attribute values
            Images and SKU
            Availability status
            Order Management:
            Customers can place orders with multiple items
            Orders can be split into SubOrders (by seller)
            Support for coupons and discounts
            Tracking of order status and payment information
            Additional Features:
            Affiliate marketing system with commission tracking
            Product rating and review system
            Search history and product recommendations
            Trending products tracking
            Cart and wishlist management
            Coupon system
            This structure allows for:

            Flexible product categorization
            Detailed product variant management
            Complex pricing and inventory control
            Multi-seller marketplace functionality
            Customer engagement features
            Advanced search and filtering capabilities
            The database is designed to handle complex e-commerce operations while maintaining data integrity and relationships between different entities.
            Your task is to:
            1. Generate safe, read-only SQL queries (SELECT only)
            2. Only use the tables and columns provided in the schema
            3. Always include appropriate JOINs and WHERE clauses
            4. Return only the SQL query, no explanation
            5. Use proper parameter placeholders (@param)
            6. Use TOP clause for result limiting (NOT LIMIT - it's not valid in T-SQL)
            7. Order results logically based on the question

            Rules:
            - ONLY generate MS SQL Server queries
            - NO DELETE, UPDATE, INSERT, or other modifying queries
            - NO accessing sensitive data (passwords, tokens)
            - NO dynamic SQL or string concatenation
            - Always use parameterized queries
            - Maximum limit of 100 rows unless specifically requested
            - Include error handling considerations
            - In case of using LIKE to search about product i want you to search also the subcategories and their description
            - Don't include ```SQL in the start of the query and ``` in the end
            ";
        }

        public string GetDatabaseSchema()
        {
            var tables = _context.Model.GetEntityTypes()
                .Select(t => new
                {
                    TableName = t.GetTableName(),
                    Columns = t.GetProperties()
                        .Select(p => new
                        {
                            Name = p.GetColumnName(),
                            Type = p.GetColumnType(),
                            IsKey = p.IsPrimaryKey()
                        })
                });

            return JsonSerializer.Serialize(tables, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }

        public QueryValidationResult ValidateGeneratedQuery(string query)
        {
            try
            {
                // Basic validation checks
                if (string.IsNullOrEmpty(query))
                    return new QueryValidationResult { IsValid = false, Message = "Query is empty" };

                query = query.Trim().ToUpper();

                // Check for forbidden operations
                var forbiddenKeywords = new[] { "DELETE", "DROP", "UPDATE", "INSERT", "EXEC", "EXECUTE" };
                if (forbiddenKeywords.Any(keyword => query.Contains(keyword)))
                {
                    return new QueryValidationResult
                    {
                        IsValid = false,
                        Message = "Query contains forbidden operations"
                    };
                }

                // Ensure it's a SELECT query
                //if (!query.StartsWith("SELECT"))
                //{
                //    return new QueryValidationResult
                //    {
                //        IsValid = false,
                //        Message = "Only SELECT queries are allowed"
                //    };
                //}

                // Validate against allowed tables
                var allowedTables = _context.Model.GetEntityTypes()
                    .Select(t => t.GetTableName().ToUpper())
                    .ToList();

                var tablePattern = @"FROM\s+([a-zA-Z_][a-zA-Z0-9_]*)|JOIN\s+([a-zA-Z_][a-zA-Z0-9_]*)";
                var matches = Regex.Matches(query, tablePattern, RegexOptions.IgnoreCase);

                foreach (Match match in matches)
                {
                    var tableName = (match.Groups[1].Value + match.Groups[2].Value).ToUpper();
                    if (!allowedTables.Contains(tableName))
                    {
                        return new QueryValidationResult
                        {
                            IsValid = false,
                            Message = $"Table {tableName} is not allowed"
                        };
                    }
                }

                // Try parsing the query with EF Core
                try
                {
                    _context.Database.CreateExecutionStrategy().Execute(() =>
                    {
                        using var command = _context.Database.GetDbConnection().CreateCommand();
                        command.CommandText = query;
                        command.CommandType = CommandType.Text;
                        return 0;
                    });
                }
                catch (Exception ex)
                {
                    return new QueryValidationResult
                    {
                        IsValid = false,
                        Message = $"Invalid query structure: {ex.Message}"
                    };
                }

                return new QueryValidationResult
                {
                    IsValid = true,
                    SafeQuery = query,
                    Message = "Query validated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating query");
                return new QueryValidationResult
                {
                    IsValid = false,
                    Message = "Error during query validation"
                };
            }
        }

        public async Task<object> ExecuteQueryAsync(string query)
        {
            try
            {
                var validationResult = ValidateGeneratedQuery(query);
                if (!validationResult.IsValid)
                {
                    throw new InvalidOperationException(validationResult.Message);
                }

                var result = await _context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
                {
                    using var command = _context.Database.GetDbConnection().CreateCommand();
                    command.CommandText = validationResult.SafeQuery;
                    command.CommandType = CommandType.Text;

                    if (command.Connection.State != ConnectionState.Open)
                        await command.Connection.OpenAsync();

                    using var reader = await command.ExecuteReaderAsync();
                    var dataTable = new DataTable();
                    dataTable.Load(reader);
                    return dataTable;
                });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing query");
                throw;
            }
        }

        public async Task<string> FormatResultsToNaturalLanguage(object results, string originalQuestion)
        {
            try
            {
                if (results is not DataTable dataTable)
                    throw new ArgumentException("Results must be a DataTable");

                var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.System, "You are a helpful assistant that converts SQL query results into natural language responses."),
            new ChatMessage(ChatRole.User, $"Question: {originalQuestion}\nResults (in JSON):\n{JsonSerializer.Serialize(dataTable)}")
        };

                var chatRequest = new ChatCompletionRequest
                {
                    Messages = messages,
                    Model = "gpt-4",
                    Temperature = 0.7,
                    MaxTokens = 500
                };

                var response = await _openAIClient.CreateChatCompletionAsync(chatRequest);
                return response.Choices[0].Message.Content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error formatting results to natural language");
                throw;
            }
        }
    }
}
