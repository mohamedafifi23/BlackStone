using Core.Entities.Identity;
using Core.IServices;
using Infrastructure.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class AdminTokenService: IAdminTokenService
    {
        private readonly IConfiguration _config;
        private readonly UserManager<Admin> _adminUserManager;
        private readonly ILogger<AdminTokenService> _logger;
        private readonly AdminIdentityDbContext _adminIdentityDbContext;
        private readonly SymmetricSecurityKey _key;

        public AdminTokenService(IConfiguration config, UserManager<Admin> adminUserManager
            , ILogger<AdminTokenService> logger, AdminIdentityDbContext adminIdentityDbContext)
        {
            _config = config;
            _adminUserManager = adminUserManager;
            _logger = logger;
            _adminIdentityDbContext = adminIdentityDbContext;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Token:Key"]));
        }

        public async Task<string> CreateTokenAsync(Admin user)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.GivenName, user.DisplayName)
            };
            var userRoles = await _adminUserManager.GetRolesAsync(user);
            var claimRoles = userRoles.Select(role => new Claim(ClaimTypes.Role, role));
            claims.AddRange(claimRoles);

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddMinutes(int.Parse(_config["Token:AccessTokenExpirationMinutes"])),
                SigningCredentials = creds,
                Issuer = _config["Token:Issuer"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public async Task<string> GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                _logger.LogInformation(string.Join(' ', "refreshToken: "+Convert.ToBase64String(randomNumber)));
                return Convert.ToBase64String(randomNumber);
            }
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters()
            {
                ValidateAudience = false,
                ValidateIssuer = true,
                ValidIssuer = _config["Token:Issuer"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _key,
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;

            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512))
                throw new SecurityTokenException("Invalid token");
            return principal;
        }

        public async Task SaveRefreshTokenAsync(RefreshToken refreshToken)
        {
            await _adminIdentityDbContext.RefreshTokens.AddAsync(refreshToken);

            await _adminIdentityDbContext.SaveChangesAsync();
        }

        public async Task SaveRefreshTokenAsync(string email)
        {
            var newRefreshToken = new RefreshToken()
            {
                Email = email,
                Token = await GenerateRefreshToken(),
                ExpiryTime = DateTime.UtcNow.AddDays(int.Parse(_config["Token:RefreshTokenExpirationDays"]))
            };

            await _adminIdentityDbContext.RefreshTokens.AddAsync(newRefreshToken);

            await _adminIdentityDbContext.SaveChangesAsync();
        }

        public async Task<RefreshToken?> CheckValidRefreshToken(string email, string refreshToken)
        {
            var token = await _adminIdentityDbContext.RefreshTokens.SingleOrDefaultAsync(t => t.Email == email);

            if (token == null || token.Token != refreshToken || token.ExpiryTime < DateTime.UtcNow)
                return null;

            return token;
        }

        public async Task<RefreshToken> UpdateRefreshTokenAsync(RefreshToken refreshTokenToUpdate)
        {
            refreshTokenToUpdate.ExpiryTime = DateTime.UtcNow.AddDays(int.Parse(_config["Token:RefreshTokenExpirationDays"]));

            _adminIdentityDbContext.RefreshTokens.Update(refreshTokenToUpdate);
            await _adminIdentityDbContext.SaveChangesAsync();

            return refreshTokenToUpdate;
        }

        public async Task<RefreshToken> UpdateRefreshTokenAsync(string Email)
        {
            var refreshTokenToUpdate = await _adminIdentityDbContext.RefreshTokens.SingleOrDefaultAsync(t => t.Email == Email);

            refreshTokenToUpdate.Token = await GenerateRefreshToken();
            refreshTokenToUpdate.ExpiryTime = DateTime.UtcNow.AddDays(int.Parse(_config["Token:RefreshTokenExpirationDays"]));

            _adminIdentityDbContext.RefreshTokens.Update(refreshTokenToUpdate);
            await _adminIdentityDbContext.SaveChangesAsync();

            return refreshTokenToUpdate;
        }

        public async Task<RefreshToken> GetRefreshTokenByEmailAsync(string email)
        {
            return await _adminIdentityDbContext.RefreshTokens.SingleOrDefaultAsync(t => t.Email == email);
        }
    }
}
