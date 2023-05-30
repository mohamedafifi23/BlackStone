using API.Helpers.CustomTokenProviders;
using Microsoft.AspNetCore.Identity;
using System.Runtime.CompilerServices;

namespace API.Extensions
{
    public static class CustomIdentityBuilderExtensions
    {
        public static IdentityBuilder AddEmailConfirmationTotpTokenProvider(this IdentityBuilder builder)
        {
            var userType = builder.UserType;
            var totpProvider = typeof(EmailConfirmationTotpTokenProvider<>).MakeGenericType(userType);
            return builder.AddTokenProvider("EmailConfirmationTotpProvider", totpProvider);
        }
    }
}
