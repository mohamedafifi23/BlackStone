using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace API.Helpers.CustomTokenProviders
{
    public class AdminEmailConfirmationTokenProvider<TUser> : DataProtectorTokenProvider<TUser> where TUser : class
    {
        public AdminEmailConfirmationTokenProvider(IDataProtectionProvider dataProtectionProvider,
            IOptions<AdminEmailConfirmationTokenProviderOptions> options,
            ILogger<DataProtectorTokenProvider<TUser>> logger)
            : base(dataProtectionProvider, options, logger)
        {
        }
    }
    public class AdminEmailConfirmationTokenProviderOptions : DataProtectionTokenProviderOptions
    {
    }
}
