using Core.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Config
{
    public class AppUserConfiguration: IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.HasIndex(x => x.Email)
                .HasDatabaseName("EmailConstraint")
                .IsUnique(true);

            builder.HasIndex(x => x.DisplayName)
                .IsUnique(false)
                .HasDatabaseName("DisplayNameConstraint");
        }
    }
}
