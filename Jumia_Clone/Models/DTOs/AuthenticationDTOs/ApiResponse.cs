namespace Jumia_Clone.Models.DTOs.AuthenticationDTOs
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; }
        public T Data { get; set; }

        public ApiResponse(T data, string message = null, bool success = true)
        {
            Success = success;
            Message = message;
            Data = data;
        }

        public ApiResponse(string message = null, bool success = true)
        {
            Success = success;
            Message = message;
        }
    }
}
