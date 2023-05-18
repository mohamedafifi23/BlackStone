using API.Errors;
using Azure;

namespace API.Middlewares
{
    public class MethodNotAllowedMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<MethodNotAllowedMiddleware> _logger;

        public MethodNotAllowedMiddleware(RequestDelegate next, ILogger<MethodNotAllowedMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);

                if (context.Response.StatusCode == StatusCodes.Status405MethodNotAllowed)
                {
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new ApiResponse(405));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }             
        }
    }
}
