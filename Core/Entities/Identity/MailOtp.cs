using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities.Identity
{
    public class MailOtp
    {
        public string Email { get; set; }
        public string Otp { get; set; }
        public DateTime ExpireTime { get; set; }
        public string Token { get; set; }
    }
}
