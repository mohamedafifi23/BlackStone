﻿using Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.IServices
{
    public interface ITokenService
    {
        Task<string> CreateToken(AppUser user);
    }
}
