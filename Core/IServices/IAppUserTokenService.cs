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
    public interface IAppUserTokenService:ITokenService
    {
        Task<string> CreateTokenAsync(AppUser user);       
    }
}
