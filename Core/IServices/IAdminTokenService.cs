using Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.IServices
{
    public interface IAdminTokenService: ITokenService
    {
        Task<string> CreateTokenAsync(Admin admin);
    }
}
