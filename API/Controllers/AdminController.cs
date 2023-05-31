using API.Dtos;
using API.Errors;
using API.Extensions;
using AutoMapper;
using Core.Entities.Identity;
using Core.IServices;
using Core.ServiceHelpers.EmailSenderService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Security.Claims;
using System.Web;

namespace API.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminController : BaseApiController
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IAppUserTokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly IEmailSenderService _emailService;
        private readonly ILogger<AdminController> _logger;
        private readonly IStringLocalizer<SharedResource> _sharedResStrLocalizer;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailSenderService _emailSenderService;

        public AdminController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager
            , IAppUserTokenService tokenService, IMapper mapper, IEmailSenderService emailService
            , ILogger<AdminController> logger, IStringLocalizer<SharedResource> sharedResStrLocalizer
            , IEmailSenderService emailSenderService)
        {
            _signInManager = signInManager;
            _tokenService = tokenService;
            _mapper = mapper;
            _emailService = emailService;
            _logger = logger;
            _sharedResStrLocalizer = sharedResStrLocalizer;
            _userManager = userManager;
            _emailSenderService = emailSenderService;
        }

        [Authorize]
        [HttpGet("getcurrentadmin")]
        public async Task<IActionResult> GetCurrentAdmin()
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

        [HttpGet("adminemailexists")]
        public async Task<ActionResult<bool>> CheckEmailExistsAsync([FromQuery] string email)
        {
            return await _userManager.FindByEmailAsync(email) != null;
        }

        [Authorize]
        [HttpGet("adminaddress")]
        public async Task<IActionResult> GetAdminAddress()
        {
            var user = await _userManager.FindUserByClaimsPrincipalWithAddress(User);

            return Ok(new ApiSuccessResponse<AddressDto>(200)
            {
                Data = _mapper.Map<Address, AddressDto>(user.Address)
            });
        }

        [HttpPut("adminaddress")]
        public async Task<IActionResult> UpdateAdminAddress(AddressDto address)
        {
            var user = await _userManager.FindUserByClaimsPrincipalWithAddress(HttpContext.User);

            user.Address = _mapper.Map<AddressDto, Address>(address);

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded) return Ok(new ApiSuccessResponse<AddressDto>(200)
            {
                Data = _mapper.Map<Address, AddressDto>(user.Address)
            });

            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "problem updating user"));
        }

        [AllowAnonymous]
        [HttpPost("admin_login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null) return Unauthorized(new ApiResponse(401));

            if (!(await _userManager.IsInRoleAsync(user, "Admin") || await _userManager.IsInRoleAsync(user, "SuperAdmin")))
                return BadRequest(new ApiResponse(400));

            var isEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
            if (!isEmailConfirmed) return StatusCode(403, new ApiResponse(
                403, _sharedResStrLocalizer["login_unconfirmedemail"]));

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded) return Unauthorized(new ApiResponse(401));

            var refreshToken = await _tokenService.GetRefreshTokenByEmailAsync(user.Email);
            if (refreshToken == null)
            {
                await _tokenService.SaveRefreshTokenAsync(user.Email);
                refreshToken = await _tokenService.GetRefreshTokenByEmailAsync(user.Email);
            }
            else
            {
                refreshToken = await _tokenService.UpdateRefreshTokenAsync(user.Email);
            }

            return Ok(new ApiSuccessResponse<AuthenticatedResponseDto>(200)
            {
                Data = new AuthenticatedResponseDto
                {
                    DisplayName = user.DisplayName,
                    Email = user.Email,
                    AccessToken = await _tokenService.CreateTokenAsync(user),
                    RefreshToken = refreshToken.Token
                }
            });
        }

        [HttpPost("registeradmin")]
        public async Task<IActionResult> Register(RegisterDto registerDto, [FromQuery][Url] string clientURI)
        {
            if (clientURI == null) return BadRequest(new ApiValidationErrorResponse()
            {
                Errors = new List<string> { "you must enter client Url" }
            });

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


            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            Dictionary<string, string> queryParams = new Dictionary<string, string>
            {
                {"email", user.Email},
                {"token", token }
            };

            var confirmationLink = QueryHelpers.AddQueryString(clientURI, queryParams);

            _logger.LogInformation(confirmationLink);

            var message = new Message(new List<string> { user.Email }, "BlackStone confirmation email link", confirmationLink);
            await _emailService.SendEmailAsync(message);

            await _userManager.AddToRoleAsync(user, "Visitor");

            return CreatedAtAction("register", "admin", new ApiSuccessResponse<UserDto>(201, "Please check your email for the verification action.")
            {
                Data = new UserDto
                {
                    DisplayName = user.DisplayName,
                    Email = user.Email
                }
            });
        }

        [AllowAnonymous]
        [HttpGet("confirmemail")]
        public async Task<IActionResult> ConfirmEmail([FromQuery][EmailAddress] string email, [FromQuery] string token)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null) return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest));

            if (user.EmailConfirmed) return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "email already confirmed"));

            var identityResult = await _userManager.ConfirmEmailAsync(user, token);

            if (!identityResult.Succeeded) return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest));

            if (await _userManager.IsInRoleAsync(user, "Visitor"))
            {
                var removeRoleResult = await _userManager.RemoveFromRoleAsync(user, "Visitor");

                if (!removeRoleResult.Succeeded) return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse(StatusCodes.Status500InternalServerError));
            }

            if (!(await _userManager.IsInRoleAsync(user, "Admin")))
            {
                var memberRoleResult = await _userManager.AddToRoleAsync(user, "Admin");
                if (!memberRoleResult.Succeeded) return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse(StatusCodes.Status500InternalServerError));
            }

            return NoContent();
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpGet("resendconfirmemailtoken")]
        public async Task<IActionResult> ResendTokenToVerifyEmail(ForgetPasswordDto forgetPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(forgetPasswordDto.Email);

            if (user == null) return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest));

            if (user.EmailConfirmed) return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "email already confirmed"));

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            Dictionary<string, string> queryParams = new Dictionary<string, string>
            {
                {"email", user.Email},
                {"token", token }
            };

            var confirmationLink = QueryHelpers.AddQueryString(forgetPasswordDto.ClientURI, queryParams);

            _logger.LogInformation(confirmationLink);

            var message = new Message(new List<string> { user.Email }, "BlackStone confirmation email link", confirmationLink);
            await _emailService.SendEmailAsync(message);

            return Ok(new ApiResponse(200, success: true));
        }     

        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("deleteadmin")]
        public async Task<IActionResult> DeleteAdmin(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null) return BadRequest(new ApiResponse(400, "user not found"));

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded) return BadRequest(new ApiResponse(400));

            return Ok(new ApiSuccessResponse<Dictionary<string, string>>(200, "user deleted successfully")
            {
                Data = new Dictionary<string, string> { { "email", email } }
            });
        }

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

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("forgetpassword")]
        public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordDto forgetPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(forgetPasswordDto.Email);

            if (user is null) return BadRequest(new ApiResponse(400, "user not found."));

            if (!user.EmailConfirmed) return BadRequest(new ApiResponse(400, "confirm your email to reset password"));

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            _logger.LogInformation(token);

            Dictionary<string, string> param = new Dictionary<string, string>
            {
                ["email"] = forgetPasswordDto.Email,
                ["token"] = token
            };
            var resetpasswordLink = QueryHelpers.AddQueryString(forgetPasswordDto.ClientURI, param);

            _logger.LogInformation(resetpasswordLink);

            var message = new Message(new List<string> { user.Email }, "BlackStone reset password link", resetpasswordLink);
            await _emailService.SendEmailAsync(message);

            return Ok(new ApiResponse(200, "Please check your email for the verification action.", true));
        }

        [AllowAnonymous]
        [HttpPost("resetpassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null) return BadRequest(new ApiResponse(400, "user not found"));

            resetPasswordDto.Token = HttpUtility.UrlDecode(resetPasswordDto.Token);
            _logger.LogInformation(resetPasswordDto.Token);

            var resetPassResult = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.Password);
            if (!resetPassResult.Succeeded) return BadRequest(new ApiValidationErrorResponse()
            { Errors = resetPassResult.Errors.Select(e => e.Description) });

            return Ok(new ApiResponse(200, "password reset successfully", true));
        }     

        [AllowAnonymous]
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
            if (validRefreshToken == null)
                return BadRequest(new ApiResponse(400, "invalid refresh token"));

            var newAccessToken = await _tokenService.CreateTokenAsync(user);
            var newRefreshToken = await _tokenService.GenerateRefreshToken();

            validRefreshToken.Token = newRefreshToken;

            await _tokenService.UpdateRefreshTokenAsync(validRefreshToken);

            return Ok(new ApiSuccessResponse<AuthenticatedResponseDto>(200, "token refreshed successfully")
            {
                Data = new AuthenticatedResponseDto
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
            if (string.IsNullOrEmpty(email)) return BadRequest(new ApiResponse(400, "user not found"));

            var refreshToken = await _tokenService.GetRefreshTokenByEmailAsync(email);
            if (refreshToken == null) return BadRequest(new ApiResponse(400, "invalid refresh token"));

            refreshToken.Token = null;

            await _tokenService.UpdateRefreshTokenAsync(refreshToken);

            return Ok(new ApiResponse(200, "token revoked successfully", success: true));
        }

        #region USers CRUD Operations
        [HttpDelete("deleteuser")]
        public async Task<IActionResult> DeleteMember(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (_userManager.IsInRoleAsync(user, "SuperAdmin").Result || _userManager.IsInRoleAsync(user, "SuperAdmin").Result)
                return BadRequest(new ApiResponse(400, "user not found"));

            if (user == null) return BadRequest(new ApiResponse(400, "user not found"));

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded) return BadRequest(new ApiResponse(400));

            return Ok(new ApiSuccessResponse<Dictionary<string, string>>(200, "user deleted successfully")
            {
                Data = new Dictionary<string, string> { { "email", email } }
            });
        }

        [HttpGet("getusers")]
        public async Task<IActionResult> GetUsers()
        {
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var superAdmin = await _userManager.GetUsersInRoleAsync("SuperAdmin");
            var users = await _userManager.Users.Include(u => u.Address)
                .ToListAsync()
                .ContinueWith(t => t.Result.Except(admins).Except(superAdmin).ToList());

            return Ok(new ApiSuccessResponse<List<UserWithAddressDto>>
            {
                Data = _mapper.Map<List<AppUser>, List<UserWithAddressDto>>(users)
            });
        }
        #endregion
        #region Users Activation
        [HttpPost("activateusers")]
        public async Task<IActionResult> ActivateUsers([FromBody] List<string> emailsToActivate)
        {
            var users = await _userManager.Users.Where(u => emailsToActivate.Contains(u.Email)).ToListAsync();
            
            foreach (var user in users)
            {
                if(await _userManager.IsInRoleAsync(user, "Visitor"))
                {
                    await _userManager.RemoveFromRoleAsync(user, "Visitor");
                    await _userManager.AddToRoleAsync(user, "Member");
                }                
            }

            var Message = new Message(users.Select(u => u.Email).ToList(), "BalckStone Account Activation", "your account is activated.");
            await _emailService.SendEmailAsync(Message);
           
            return Ok(new ApiSuccessResponse<List<string>>(200, "users activated successfully")
            {
                Data = emailsToActivate
            });
        }

        [HttpPost("deactivateusers")]
        public async Task<IActionResult> DeactivateUsers([FromBody] List<string> emailsToDeactivate)
        {
            var users = await _userManager.Users.Where(u => emailsToDeactivate.Contains(u.Email)).ToListAsync();
            
            foreach (var user in users)
            {
                if (await _userManager.IsInRoleAsync(user, "Member"))
                {
                    await _userManager.RemoveFromRoleAsync(user, "Member");
                    await _userManager.AddToRoleAsync(user, "Visitor");
                }
            }

            var Message = new Message(users.Select(u=>u.Email).ToList(), "BalckStone Account Deactivation", "your account is deactivated.");
            await _emailService.SendEmailAsync(Message);

            return Ok(new ApiSuccessResponse<List<string>>(200, "users deactivated successfully")
            {
                Data = emailsToDeactivate
            });
        }

        [HttpGet("activeusers")]
        public async Task<IActionResult> GetActiveUsers()
        {
            var users = await _userManager.Users.Include(u => u.Address)
              .ToListAsync()
              .ContinueWith(t => t.Result.Intersect(_userManager.GetUsersInRoleAsync("Member").Result).ToList());

            return Ok(new ApiSuccessResponse<List<UserWithAddressDto>>(200)
            {
                Data = _mapper.Map<List<AppUser>, List<UserWithAddressDto>>(users.ToList())
            });
        }

        [HttpGet("nonactiveusers")]
        [HttpGet("newusers")]
        public async Task<IActionResult> GetNonActiveUsers()
        {
            var users = await _userManager.Users.Include(u => u.Address)
                .ToListAsync()
                .ContinueWith(t=> t.Result.Intersect(_userManager.GetUsersInRoleAsync("Visitor").Result).ToList());
            
            return Ok(new ApiSuccessResponse<List<UserWithAddressDto>>(200)
            {
                Data = _mapper.Map<List<AppUser>, List<UserWithAddressDto>>(users.ToList())
            });
        }
        #endregion

        #region User Groups
         
        #endregion
    }
}
