using Jumia_Clone.Models.DTOs.AiChatBotDTOs;

namespace Jumia_Clone.Services.Interfaces
{
    public interface IAIQueryService
    {
        /// <summary>
        /// Generates a SQL query from a natural language user question
        /// </summary>
        /// <param name="userQuestion">The natural language question from the user</param>
        /// <returns>A generated SQL query string</returns>
        Task<string> GenerateQueryFromUserQuestion(string userQuestion);

        /// <summary>
        /// Validates a generated query for safety and correctness
        /// </summary>
        /// <param name="query">The SQL query to validate</param>
        /// <returns>A validation result containing status and any error messages</returns>
        QueryValidationResult ValidateGeneratedQuery(string query);

        /// <summary>
        /// Executes a validated query and returns the results
        /// </summary>
        /// <param name="query">The validated SQL query to execute</param>
        /// <returns>The query results as a dynamic object</returns>
        Task<object> ExecuteQueryAsync(string query);

        /// <summary>
        /// Gets the current database schema for AI context
        /// </summary>
        /// <returns>A string representation of the database schema</returns>
        string GetDatabaseSchema();

        /// <summary>
        /// Formats query results into a natural language response
        /// </summary>
        /// <param name="results">The query results</param>
        /// <param name="originalQuestion">The original user question</param>
        /// <returns>A natural language response describing the results</returns>
        Task<string> FormatResultsToNaturalLanguage(object results, string originalQuestion);
    }
}
