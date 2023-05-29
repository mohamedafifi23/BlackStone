using Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Identity
{
    public static class AdminIdentityDbContextSeed
    {
        public static async Task SeedIdentityAsync(UserManager<Admin> userManager, RoleManager<AdminRole> roleManager)
        {
            if (!roleManager.Roles.Any())
            {
                var roles = new List<AdminRole>()
                {
                    new AdminRole(){Name="Visitor"},
                    new AdminRole(){Name="Admin"},                    
                    new AdminRole(){Name="SuperAdmin"}
                };

                foreach (var role in roles)
                {
                    await roleManager.CreateAsync(role);
                }
            }

            if (!userManager.Users.Any())
            {
                var admin = new Admin()
                {
                    DisplayName = "mohamed afifi",
                    UserName= "mohammedafifi153@gmail.com",
                    Email = "mohammedafifi153@gmail.com",
                    PhoneNumber="+0101591099",
                    AdminAddress = new AdminAddress()
                    {
                        FirstName = "mohamed",
                        MiddleName = "ashraf",
                        LastName = "afifi",
                        State = "Cairo",
                        City = "5th settlement",
                        Street = "villa 289, el-narges 2"
                    }
                };

                await userManager.CreateAsync(admin, "P@$$w0rd");

                admin.EmailConfirmed = true;
                admin.PhoneNumberConfirmed=true;
                await userManager.UpdateAsync(admin);

                await userManager.AddToRoleAsync(admin, "SuperAdmin");
            }   
        }
    }
}
