namespace API.Errors
{
    public class ApiErrorResponse: ApiResponse
    {
        public ApiErrorResponse(int statusCode, string message = null) : base(statusCode, message)
        {            
        }

        public IEnumerable<string> Errors { get; set; }
    }
}
