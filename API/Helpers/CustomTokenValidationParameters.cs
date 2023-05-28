using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace API.Helpers
{
    public class CustomTokenValidationParameters: TokenValidationParameters
    {

        public CustomTokenValidationParameters(IConfiguration config)
        {

            ValidateIssuerSigningKey = true;
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Token:Key"]));
            ValidateIssuer = true;
            ValidIssuer = config["Token:Issuer"];
            ValidateAudience = false;
        }
    }
}
