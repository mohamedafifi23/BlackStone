namespace API.Errors
{
    public class ApiOkResponse<T>: ApiResponse
    {
        public ApiOkResponse(int statusCode, string message = null, bool success = true)
            : base(statusCode, message, success)
        {
        }

        public T Data { get; set; }
    }
}
