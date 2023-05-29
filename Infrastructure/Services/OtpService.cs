using Core.Entities.Identity;
using Core.IServices;
using Core.ServiceHelpers.EmailSenderService;
using Infrastructure.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class OtpService : IOtpService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<OtpService> _logger;
        private readonly IEmailSenderService _emailService;
        private readonly UserManager<AppUser> _userManager;
        private readonly AppIdentityDbContext _appIdentityDbContext;

        public OtpService(IConfiguration config, ILogger<OtpService> logger
            , IEmailSenderService emailService, UserManager<AppUser> userManager
            , AppIdentityDbContext appIdentityDbContext)
        {
            _config = config;
            _logger = logger;
            _emailService = emailService;
            _userManager = userManager;
            _appIdentityDbContext = appIdentityDbContext;
        }        

        public string GenerateRandomNumericOTP()
        {
            try
            {
                byte otpLength = byte.Parse(_config["Otp:Length"]);
                Random rand = new Random();
                StringBuilder generatedOtp = new StringBuilder();

                for (int i = 0; i < otpLength; i++)
                {
                    generatedOtp.Append(rand.Next(0, 10));
                }

                return generatedOtp.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return "";
            }
        }

        public string GenerateRandomAlphaNumericOTP()
        {
            try
            {
                byte otpLength = byte.Parse(_config["Otp:Length"]);
                string allowedChars = _config["Otp:AllowedCharacters"];
                Random rand = new Random();
                StringBuilder generatedOtp = new StringBuilder();

                for (int i = 0; i < otpLength; i++)
                {
                    char randomChar = allowedChars[rand.Next(0, allowedChars.Length)];
                    generatedOtp.Append(randomChar);
                }

                return generatedOtp.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return "";
            }
        }

        public async Task<MailOtp> SaveUserMailOtpAsync(string email, string otp, string token)
        {
            var mailOtp = await _appIdentityDbContext.MailOtps.FirstOrDefaultAsync(o => o.Email == email);
            
            try
            {
                DateTime expireTime = DateTime.UtcNow.AddMinutes(int.Parse(_config["Otp:ExpireTime"]));

                if (mailOtp == null)
                {
                    mailOtp = new MailOtp
                    {
                        Email = email,
                        Otp = otp,
                        ExpireTime = expireTime,
                        Token = token
                    };
                    await _appIdentityDbContext.MailOtps.AddAsync(mailOtp);
                }
                else
                {
                    mailOtp.Otp = otp;
                    mailOtp.ExpireTime = expireTime;
                    mailOtp.Token = token;

                    _appIdentityDbContext.MailOtps.Update(mailOtp);
                }

                _appIdentityDbContext.Attach(mailOtp);
                _appIdentityDbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            //mailOtp.Otp = otp;
            return mailOtp;
        }

        public async Task<bool> SendMailOtpAsync(string email, string subject, string content)
        {
            try
            {
                Message message = new Message(new List<string> { email }, subject, content);
                await _emailService.SendEmailAsync(message);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return false;
        }

        public async Task<MailOtp> VerifyUserMailOtpAsync(string email, string otp)
        {
            try
            {
                var mailOtp = await _appIdentityDbContext.MailOtps.FirstOrDefaultAsync(o => o.Email == email && o.Otp == otp);
                if (mailOtp is MailOtp)
                {
                    if (mailOtp.ExpireTime >= DateTime.UtcNow)
                        return mailOtp;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new MailOtp();
            }

            return new MailOtp();
        }

        public async Task<string> GetTokenOfVerifiedMailOtpAsync(string email, string otp)
        {
            var mailOtp = await _appIdentityDbContext.MailOtps.FirstOrDefaultAsync(o => o.Email == email && o.Otp == otp);

            return mailOtp.Token;
        }

        public async Task DeleteUserVerifiedOtpAsync(string email, string otp)
        {
            var mailOtp = await _appIdentityDbContext.MailOtps.FirstOrDefaultAsync(o => o.Email == email && o.Otp == otp);

            _appIdentityDbContext.MailOtps.Remove(mailOtp);
            _appIdentityDbContext.SaveChanges();
        }
    }
}
