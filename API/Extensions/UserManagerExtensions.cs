using Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace API.Extensions
{
    public static class UserManagerExtensions
    {
        public async static Task<AppUser> FindUserByClaimsPrincipalWithAddress(this UserManager<AppUser> userManager,
            ClaimsPrincipal user)
        {
            var email = user.FindFirstValue(ClaimTypes.Email);

            return await userManager.Users.Include(u=>u.Address)
                .SingleOrDefaultAsync(u=>u.Email == email);
        }

        public static async Task<AppUser> FindByEmailFromClaimsPrincipal(this UserManager<AppUser> userManager, 
            ClaimsPrincipal user)
        {
            var email = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            return await userManager.FindByEmailAsync(email);
        }

        public async static Task<AppUser> FindUserByEmailWithAddress(this UserManager<AppUser> userManager, 
            [EmailAddress] string email)
        {
            return await userManager.Users.Include(u => u.Address)
                .SingleOrDefaultAsync(u => u.Email == email);
        }       
    }
}
