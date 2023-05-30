using Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Identity
{
    public static class AppIdentityDbContextSeed
    {
        public static async Task SeedIdentityAsync(UserManager<AppUser> userManager, RoleManager<AppUserRole> roleManager)
        {
            if (!roleManager.Roles.Any())
            {
                var roles = new List<AppUserRole>()
                {
                    new AppUserRole(){Name="Visitor"},
                    new AppUserRole(){Name="Member"}
                };

                foreach (var role in roles)
                {
                    await roleManager.CreateAsync(role);
                }
            }

            if (!userManager.Users.Any())
            {
                var appUser = new AppUser()
                {
                    DisplayName = "mohamed afifi",
                    UserName= "mohammedafifi153@gmail.com",
                    Email = "mohammedafifi153@gmail.com",
                    PhoneNumber="+0101591099",
                    Address = new Address()
                    {
                        FirstName = "mohamed",
                        MiddleName = "ashraf",
                        LastName = "afifi",
                        State = "Cairo",
                        City = "5th settlement",
                        Street = "villa 289, el-narges 2"
                    }
                };

                await userManager.CreateAsync(appUser, "P@$$w0rd");

                appUser.EmailConfirmed = true;
                appUser.PhoneNumberConfirmed=true;
                await userManager.UpdateAsync(appUser);

                await userManager.AddToRoleAsync(appUser, "Member");
            }   
        }
    }
}
