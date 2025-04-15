namespace Jumia_Clone.CustomException
{
    public class ApiException : Exception
    {
        public int StatusCode { get; }
        public string ErrorCode { get; }

        public ApiException(string message, int statusCode = 500, string errorCode = "INTERNAL_ERROR")
            : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }
    }
}
