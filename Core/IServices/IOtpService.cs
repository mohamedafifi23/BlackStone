using Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.IServices
{
    public interface IOtpService
    {
        string GenerateRandomNumericOTP();
        string GenerateRandomAlphaNumericOTP();
        Task<MailOtp> SaveUserMailOtpAsync(string email, string otp, string token);
        Task<bool> SendMailOtpAsync(string email, string subject, string content);
        Task<MailOtp> VerifyUserMailOtpAsync(string email, string otp);
        Task<string> GetTokenOfVerifiedMailOtpAsync(string email, string otp);
        Task DeleteUserVerifiedOtpAsync(string email, string otp);
    }
}
