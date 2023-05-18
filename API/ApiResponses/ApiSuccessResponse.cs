namespace API.Errors
{
    public class ApiSuccessResponse<T>: ApiResponse
    {
        public ApiSuccessResponse(int statusCode = 200, string message = null, bool success = true)
            : base(statusCode, message, success)
        {
        }

        public T Data { get; set; }
    }
}
