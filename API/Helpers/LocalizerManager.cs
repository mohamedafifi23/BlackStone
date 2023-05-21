using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace API.Helpers
{
    public static class LocalizerManager
    {
        public static IStringLocalizer<SharedResource> SharedResourceLocalizer;

        public static void SetLocalizer(IStringLocalizer<SharedResource> sharedResourceLocalizer)
        {
            SharedResourceLocalizer = sharedResourceLocalizer;
        }
    }
}
