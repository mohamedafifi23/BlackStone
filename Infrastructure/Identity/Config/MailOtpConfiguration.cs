using Core.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Identity.Config
{
    public class MailOtpConfiguration : IEntityTypeConfiguration<MailOtp>
    {
        public void Configure(EntityTypeBuilder<MailOtp> builder)
        {
            builder.HasKey(o => new { o.Email });
        }
    }
}
