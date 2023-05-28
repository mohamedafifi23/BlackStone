using Core.Entities.Identity;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Core.IServices
{
    public interface ITokenService
    {
        Task<string> CreateTokenAsync(AppUser user);
        Task<string> GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        Task SaveRefreshTokenAsync(RefreshToken refreshToken);
        Task SaveRefreshTokenAsync(string email);
        Task<RefreshToken?> CheckValidRefreshToken(string email, string refreshToken);
        Task<RefreshToken> UpdateRefreshTokenAsync(RefreshToken refreshTokenToUpdate);
        Task<RefreshToken> UpdateRefreshTokenAsync(string Email);
        Task<RefreshToken> GetRefreshTokenByEmailAsync(string email);
    }
}
