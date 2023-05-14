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
            if (!userManager.Users.Any())
            {
                var appUser = new AppUser()
                {
                    DisplayName = "mohamed afifi",
                    UserName= "mohammedafifi153@gmail.com",
                    Address = new Address()
                    {
                        FirstName = "mohamed",
                        MiddleName = "ashraf",
                        LastName = "afifi",
                        Email = "mohammedafifi153@gmail.com",
                        State = "Cairo",
                        City = "5th settlement",
                        StreetAddress = "villa 289, el-narges 2"
                    }
                };

                await userManager.CreateAsync(appUser, "P@$$w0rd");
            }
        }
    }
}
