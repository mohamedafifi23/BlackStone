using API.Errors;

namespace API.Middlewares
{
    public class NoContentMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<NoContentMiddleware> _logger;

        public NoContentMiddleware(RequestDelegate next, ILogger<NoContentMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);

                if (context.Response.StatusCode == StatusCodes.Status204NoContent)
                {
                    context.Response.ContentType = "application/json";
                    //if status code not changed, it will throw an exception "you can not write on body of no content response"
                    context.Response.StatusCode = StatusCodes.Status200OK;   
                    await context.Response.WriteAsJsonAsync(new ApiResponse(200,success: true));
                }

                //await _next(context); // won't change anything              
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
    }
}
