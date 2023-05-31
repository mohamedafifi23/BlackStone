using API.Errors;
using Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace API.Filters
{
    public class NonAdminUserFilter : IAsyncActionFilter
    {
        private readonly UserManager<AppUser> _userManager;

        public NonAdminUserFilter(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var userEmail = context.ActionArguments.TryGetValue("email", out object result);

            AppUser user;
            //List<AppUser> users;    

            if (result is string)
            {
                user = await _userManager.FindByEmailAsync(result.ToString());

                if (await _userManager.IsInRoleAsync(user, "Admin") || await _userManager.IsInRoleAsync(user, "SuperAdmin"))
                    context.Result = new BadRequestObjectResult(new ApiResponse(400));

            }

            //if (result is List<string>)
            //{
            //    var emails = result as List<string>;
            //    users = await _userManager.Users.Where(u => emails.Contains(u.Email)).ToListAsync();

            //    if (await _userManager.IsInRoleAsync(user, "Admin") || await _userManager.IsInRoleAsync(user, "SuperAdmin"))
            //        context.Result = new BadRequestObjectResult(new ApiResponse(400));

            //}

            await next();
        }
    }
}
