using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
namespace Jumia_Clone.CustomException
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            // Log the exception
            _logger.LogError(exception, "An unhandled exception occurred.");

            // Handle specific exception types
            var errorResponse = exception switch
            {
                ApiException apiEx => new ErrorResponse(
                    apiEx.Message,
                    apiEx.ErrorCode,
                    new List<string> { apiEx.StackTrace }
                ),

               
                InsufficientStockException stockEx => new ErrorResponse(
                    stockEx.Message,
                    "INSUFFICIENT_STOCK",
                    new List<string> {
                    $"Requested: {stockEx.RequestedQuantity}, " +
                    $"Available: {stockEx.AvailableStock}"
                    }
                ),

                DbUpdateException dbEx => new ErrorResponse(
                    "Database operation failed",
                    "DATABASE_ERROR",
                    new List<string> { dbEx.InnerException?.Message }
                ),

                // Default catch-all for unhandled exceptions
                _ => new ErrorResponse(
                    "An unexpected error occurred",
                    "UNEXPECTED_ERROR",
                    new List<string> { exception.Message }
                )
            };

            // Set the response status code
            httpContext.Response.StatusCode = exception switch
            {
                ApiException apiEx => apiEx.StatusCode,
                ValidationException => StatusCodes.Status400BadRequest,
                DbUpdateException => StatusCodes.Status500InternalServerError,
                _ => StatusCodes.Status500InternalServerError
            };

            // Write the error response
            await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);

            return true;
        }
    }
}
