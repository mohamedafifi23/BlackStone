using API.Controllers;
using Microsoft.Extensions.Localization;

namespace API.Errors
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        private static IStringLocalizer<SharedResource> _localizer;

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
                200 => _localizer["200"],
                201 => _localizer["201"],
                400 => _localizer["400"],
                401 => _localizer["401"],
                403 => _localizer["403"],
                404 => _localizer["404"],
                405 => _localizer["405"],
                409 => _localizer["409"],
                415 => _localizer["415"],
                422 => _localizer["422"],
                500 => _localizer["500"],
                _ => null           
            };
        }

        public static void SetLocalizer(IStringLocalizer<SharedResource> localizer)
        {
            _localizer = localizer;
        }
    }
}
