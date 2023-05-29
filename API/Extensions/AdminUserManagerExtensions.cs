using Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace API.Extensions
{
    public static class AdminUserManagerExtensions
    {
        public async static Task<Admin> FindUserByClaimsPrincipalWithAddress(this UserManager<Admin> userManager,
            ClaimsPrincipal user)
        {
            var email = user.FindFirstValue(ClaimTypes.Email);

            return await userManager.Users.Include(u=>u.AdminAddress)
                .SingleOrDefaultAsync(u=>u.Email == email);
        }

        public static async Task<Admin> FindByEmailFromClaimsPrincipal(this UserManager<Admin> userManager, 
            ClaimsPrincipal user)
        {
            var email = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            return await userManager.FindByEmailAsync(email);
        }

        public async static Task<Admin> FindUserByEmailWithAddress(this UserManager<Admin> userManager, 
            [EmailAddress] string email)
        {
            return await userManager.Users.Include(u => u.AdminAddress)
                .SingleOrDefaultAsync(u => u.Email == email);
        }       
    }
}
