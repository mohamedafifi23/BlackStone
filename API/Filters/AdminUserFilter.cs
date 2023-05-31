using API.Errors;
using Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Filters
{
    public class AdminUserFilter : IAsyncActionFilter
    {
        private readonly UserManager<AppUser> _userManager;

        public AdminUserFilter(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var userEmail = context.ActionArguments.TryGetValue("email", out object email);
            AppUser user;
            if(userEmail is string)
            {
                user = await _userManager.FindByEmailAsync(userEmail.ToString());

                if (await _userManager.IsInRoleAsync(user, "Admin") || await _userManager.IsInRoleAsync(user, "SuperAdmin"))
                    context.Result = new BadRequestObjectResult(new ApiResponse(400));

            }

            if (userEmail is List<string>)
            {
                user = await _userManager.FindByEmailAsync(userEmail.ToString());

                if (await _userManager.IsInRoleAsync(user, "Admin") || await _userManager.IsInRoleAsync(user, "SuperAdmin"))
                    context.Result = new BadRequestObjectResult(new ApiResponse(400));

            }

            await next();
        }
    }
}
