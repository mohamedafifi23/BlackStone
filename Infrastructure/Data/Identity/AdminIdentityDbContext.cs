using Core.Entities.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Identity
{
    public class AdminIdentityDbContext: IdentityDbContext<Admin, AdminRole, string>
    {
        public AdminIdentityDbContext(DbContextOptions<AdminIdentityDbContext> adminDbContextOptions)
            :base(adminDbContextOptions) { }

        public DbSet<MailOtp> MailOtps { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
