namespace Jumia_Clone.Models.DTOs.GeneralDTOs
{
    public class ApiErrorResponse
    {
        public bool Success { get; set; } = false;
        public string Message { get; set; }
        public string[] ErrorMessages { get; set; } = new string[0];
        public ApiErrorResponse() { 
            Success = false;
        }
        public ApiErrorResponse(string[] errorMessage,string message = null)
        {
            Success = false;
            Message = message;
            ErrorMessages = errorMessage;

        }
    }
}
