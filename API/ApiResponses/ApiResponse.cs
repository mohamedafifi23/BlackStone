using API.Controllers;
using Microsoft.Extensions.Localization;

namespace API.Errors
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }

        public ApiResponse(int statusCode, string message = null, bool success = false)
        {
            StatusCode = statusCode;
            Message = message ?? GetDefaultMessage(statusCode);
            Success = success;           
        }

        private string? GetDefaultMessage(int statusCode)
        {
            return statusCode switch
            {
                200 => "Ok, you made it",
                201 => "Created, you made it",
                400 => "bad request, you made",
                401 => "Authorized, you are not authorized",
                403 => "Forbidden, you are not authorized to see this",
                404 => "Resource found, it was not found",
                405 => "Method not allowed, you are not allowed to use this method",
                409 => "Conflict, there is a conflict",
                415 => "Unsupported media type, this media type is not supported",
                422 => "Unprocessable entity, this entity is not processable",
                500 => "Something went wrong and we are going to solve it",
                _ => null
            };
        }
    }
}
