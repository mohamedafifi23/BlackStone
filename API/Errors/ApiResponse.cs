namespace API.Errors
{
    public class ApiResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }

        public ApiResponse(int statusCode, string message = null)
        {
            StatusCode= statusCode;
            Message= message ?? GetDefaultMessage(statusCode);
        }

        private string? GetDefaultMessage(int statusCode)
        {
            return statusCode switch
            {
                400 => "bad request, you made",
                401 => "Authorized, you are not authorized",
                403 => "Forbidden, I know you but you are not authorized to see this",
                404 => "Resource found, it was not found",
                500 => "Something wen wrong and we are going to solve it",
                _ => null
            };
        }
    }
}
