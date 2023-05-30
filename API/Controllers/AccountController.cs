using API.Dtos;
using API.Errors;
using API.Extensions;
using AutoMapper;
using Azure;
using Core.Entities.Identity;
using Core.IServices;
using Core.ServiceHelpers.EmailSenderService;
using Infrastructure.Data.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Web;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IAppUserTokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly IEmailSenderService _emailService;
        private readonly ILogger<AccountController> _logger;
        private readonly IStringLocalizer<SharedResource> _sharedResStrLocalizer;
        private readonly IOtpService _otpService;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager
            , IAppUserTokenService tokenService, IMapper mapper, IEmailSenderService emailService
            , ILogger<AccountController> logger, IStringLocalizer<SharedResource> sharedResStrLocalizer
            ,IOtpService otpService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _mapper = mapper;
            _emailService = emailService;
            _logger = logger;
            _sharedResStrLocalizer = sharedResStrLocalizer;
            _otpService = otpService;
        }

        [Authorize]
        [HttpGet("getcurrentuser")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var user = await _userManager.FindByEmailFromClaimsPrincipal(User);

            return Ok(new ApiSuccessResponse<UserDto>(200)
            {
                Data = new UserDto
                {
                    DisplayName = user.DisplayName,
                    Email = user.Email
                }
            });
        }
        
        [HttpGet("emailexists")]
        public async Task<ActionResult<bool>> CheckEmailExistsAsync([FromQuery] string email)
        {
            return await _userManager.FindByEmailAsync(email) != null;
        }

        [Authorize]
        [HttpGet("address")]
        public async Task<IActionResult> GetUserAddress()
        {
            var user = await _userManager.FindUserByClaimsPrincipalWithAddress(User);

            return Ok(new ApiSuccessResponse<AddressDto>(200)
            {
                Data= _mapper.Map<Address, AddressDto>(user.Address)
            });
        }

        [Authorize]
        [HttpPut("address")]
        public async Task<IActionResult> UpdateUserAddress(AddressDto address)
        {
            var user = await _userManager.FindUserByClaimsPrincipalWithAddress(HttpContext.User);

            user.Address = _mapper.Map<AddressDto, Address>(address);

            var result = await _userManager.UpdateAsync(user);

            if(result.Succeeded) return Ok(new ApiSuccessResponse<AddressDto>(200)
            {
                Data= _mapper.Map<Address, AddressDto>(user.Address)
            });
 
            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "problem updating user"));    
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null) return Unauthorized(new ApiResponse(401));
 
            var isEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
            if (!isEmailConfirmed) return StatusCode(403, new ApiResponse(
                403, _sharedResStrLocalizer["login_unconfirmedemail"]));

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded) return Unauthorized(new ApiResponse(401));

            var refreshToken = await _tokenService.GetRefreshTokenByEmailAsync(user.Email);
            if(refreshToken == null)
            {
                await _tokenService.SaveRefreshTokenAsync(user.Email);
                refreshToken = await _tokenService.GetRefreshTokenByEmailAsync(user.Email);
            }
            else
            {
                refreshToken =await _tokenService.UpdateRefreshTokenAsync(user.Email);
            }

            return Ok(new ApiSuccessResponse<AuthenticatedResponseDto>(200)
            {
                Data = new AuthenticatedResponseDto
                {
                    DisplayName = user.DisplayName,
                    Email = user.Email,
                    AccessToken = await _tokenService.CreateTokenAsync(user),
                    RefreshToken=refreshToken.Token
                }
            });
        }        

        [HttpPost("registerotp")]
        public async Task<IActionResult> RegisterOtp(RegisterDto registerDto)
        {
            if (CheckEmailExistsAsync(registerDto.Email).Result.Value)
            {
                return new BadRequestObjectResult(new ApiValidationErrorResponse()
                {
                    Errors = new[] { "email address is used before" }
                });
            }

            var user = new AppUser()
            {
                Email = registerDto.Email,
                DisplayName = registerDto.DisplayName,
                UserName = registerDto.Email,
                PhoneNumber = registerDto.Phone
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded) return BadRequest(new ApiResponse(400));

            var otp = _otpService.GenerateRandomNumericOTP();
            //var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            //var token = await _userManager.GenerateUserTokenAsync(user, "EmailConfirmationTotpTokenProvider", "EmailConfirmationOtp");
            var token = Convert.ToBase64String(await _userManager.CreateSecurityTokenAsync(user));
            MailOtp mailOtp = await _otpService.SaveUserMailOtpAsync(user.Email, otp, token);
            await _otpService.SendMailOtpAsync(user.Email, "BlackStone confirmation email link", mailOtp.Otp);
                        
            await _userManager.AddToRoleAsync(user, "Visitor");

            return CreatedAtAction("registerotp", "account", new ApiSuccessResponse<UserDto>(201, "Please check your email for the verification action.")
            {
                Data = new UserDto
                {
                    DisplayName = user.DisplayName,
                    Email = user.Email
                }
            });
        }

        [HttpGet("resendconfirmemailotp")]
        public async Task<IActionResult> ResendOtpToVerifyEmail([EmailAddress] string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null) return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest));

            if (user.EmailConfirmed) return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "email already confirmed"));

            var otp = _otpService.GenerateRandomNumericOTP();
            //var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var token = Convert.ToBase64String(await _userManager.CreateSecurityTokenAsync(user));
            MailOtp mailOtp = await _otpService.SaveUserMailOtpAsync(user.Email, otp, token);
            await _otpService.SendMailOtpAsync(user.Email, "BlackStone confirmation email link", mailOtp.Otp);

            return Ok(new ApiResponse(200, success: true));
        }

        [HttpPost("confirmemailotp")]
        public async Task<IActionResult> ConfirmEmailOtp([FromBody] ConfirmEmailOtpDto confirmEmailOtpDto)
        {
            var user = await _userManager.FindByEmailAsync(confirmEmailOtpDto.Email);

            if (user == null) return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest));

            if (user.EmailConfirmed) return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "email already confirmed"));

            MailOtp valifMailOtp = await _otpService.VerifyUserMailOtpAsync(confirmEmailOtpDto.Email, confirmEmailOtpDto.Otp);
            if (string.IsNullOrEmpty(valifMailOtp.Token)) return BadRequest(new ApiResponse(400, "your email is not verified. check correctness of entered otp or it will be expired."));

            //var identityResult = await _userManager.ConfirmEmailAsync(user, valifMailOtp.Token);
            user.EmailConfirmed = true;
            var identityResult = await _userManager.UpdateAsync(user);
            if (!identityResult.Succeeded) return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest));

            if (await _userManager.IsInRoleAsync(user, "Visitor"))
            {
                var removeRoleResult = await _userManager.RemoveFromRoleAsync(user, "Visitor");

                if (!removeRoleResult.Succeeded) return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse(StatusCodes.Status500InternalServerError));
            }

            if (!await _userManager.IsInRoleAsync(user, "Member"))
            {
                var memberRoleResult = await _userManager.AddToRoleAsync(user, "Member");
                if (!memberRoleResult.Succeeded) return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse(StatusCodes.Status500InternalServerError));
            }

            await _otpService.DeleteUserVerifiedOtpAsync(confirmEmailOtpDto.Email, confirmEmailOtpDto.Otp);
            return NoContent();
        }

        [Authorize]
        [HttpPost("changepassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            var user = await _userManager.FindByEmailFromClaimsPrincipal(User);

            var checkPassword = await _userManager.CheckPasswordAsync(user, changePasswordDto.NewPassword);
            if (checkPassword) return BadRequest(new ApiResponse(400, "New password must be different from the old password."));

            var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);

            if (!result.Succeeded) return BadRequest(new ApiValidationErrorResponse()
                    { Errors = result.Errors.Select(e => e.Description) });

            return Ok(new ApiResponse(200, "Password changed successfully", true));
        }

        [HttpPost("forgetpasswordotp")]
        public async Task<IActionResult> ForgetPasswordOtp([FromBody] ForgetPasswordOtpDto forgetPasswordOtpDto)
        {
            var user = await _userManager.FindByEmailAsync(forgetPasswordOtpDto.Email);

            if (user is null) return BadRequest(new ApiResponse(400, "user not found."));

            if (!await _userManager.IsEmailConfirmedAsync(user)) return BadRequest(new ApiResponse(400, "confirm your email to reset password"));

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var otp = _otpService.GenerateRandomNumericOTP();
            MailOtp mailOtp = await _otpService.SaveUserMailOtpAsync(user.Email, otp, token);

            bool emailSent = await _otpService.SendMailOtpAsync(user.Email, "BlackStone reset password link", mailOtp.Otp);
            if (emailSent)
                return Ok(new ApiResponse(200, "Please check your email for the verification action.", true));
            
            return BadRequest(new ApiResponse(400, "something went wrong when sending verfication OTP"));
        }

        [HttpPost("resetpasswordotp")]
        public async Task<IActionResult> ResetPasswordOtp(ResetPasswordOtpDto resetPasswordOtpDto)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordOtpDto.Email);
            if (user == null) return BadRequest(new ApiResponse(400, "user not found"));
            
            var validMailOtp = await _otpService.VerifyUserMailOtpAsync(resetPasswordOtpDto.Email, resetPasswordOtpDto.Otp);
            if (string.IsNullOrEmpty(validMailOtp.Token)) return BadRequest(new ApiResponse(400, "your email is not verified. check correctness of entered otp or it will be expired."));

            var resetPassResult = await _userManager.ResetPasswordAsync(user, validMailOtp.Token, resetPasswordOtpDto.Password);
            if (!resetPassResult.Succeeded) return BadRequest(new ApiValidationErrorResponse()
            { Errors = resetPassResult.Errors.Select(e => e.Description) });

            await _otpService.DeleteUserVerifiedOtpAsync(validMailOtp.Email, validMailOtp.Otp);

            return Ok(new ApiResponse(200, "password reset successfully", true));
        }

        [HttpPost("refreshtoken")]
        public async Task<IActionResult> RefreshToken(RefreshTokenDto refreshTokenDto)
        {
            string accessToken = refreshTokenDto.AccessToken;
            string refreshToken = refreshTokenDto.RefreshToken;

            var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);

            var email = principal.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return BadRequest(new ApiResponse(400, "user not found"));

            var validRefreshToken = await _tokenService.CheckValidRefreshToken(email, refreshToken);
            if ( validRefreshToken == null)
                return BadRequest(new ApiResponse(400, "invalid refresh token"));

            var newAccessToken = await _tokenService.CreateTokenAsync(user);
            var newRefreshToken = await _tokenService.GenerateRefreshToken();

            validRefreshToken.Token = newRefreshToken;

            await _tokenService.UpdateRefreshTokenAsync(validRefreshToken);

            return Ok(new ApiSuccessResponse<AuthenticatedResponseDto>(200, "token refreshed successfully")
            {
                Data= new AuthenticatedResponseDto
                {
                    Email = user.Email,
                    DisplayName = user.DisplayName,
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken
                }
            });
        }

        [Authorize, HttpPost("revoke")]
        public async Task<IActionResult> Revoke()
        {
            string email = User.RetrieveEmailFromPrincipal();    
            if(string.IsNullOrEmpty(email)) return BadRequest(new ApiResponse(400, "user not found"));

            var refreshToken = await _tokenService.GetRefreshTokenByEmailAsync(email);
            if (refreshToken == null) return BadRequest(new ApiResponse(400, "invalid refresh token"));

            refreshToken.Token = null;

            await _tokenService.UpdateRefreshTokenAsync(refreshToken);

            return Ok(new ApiResponse(200, "token revoked successfully", success: true));
        }
    }
}
