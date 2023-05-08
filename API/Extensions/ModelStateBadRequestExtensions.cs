using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

namespace API.Extensions
{
    public static class ModelStateBadRequestExtensions
    {
        public static IServiceCollection OverrideModelStateBadRequestBehaviour(this IServiceCollection services)
        {
            return services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = actionContext =>
                {
                    //return new BadRequestObjectResult(UnifiedResponseResult.GenerateResult(
                    //    StatusCodes.Status400BadRequest, null, "bad request", actionContext.ModelState.Select(s => new { s.Key, value = s.Value.Errors.Select(e => e.ErrorMessage) })
                    //        ));

                    return null;
                };
            });

        }
    }
}
