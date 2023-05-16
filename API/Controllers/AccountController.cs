using API.Dtos;
using API.Errors;
using API.Extensions;
using AutoMapper;
using Azure;
using Core.Entities.Identity;
using Core.IServices;
using Core.ServiceHelpers.EmailSenderService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly IEmailSenderService _emailService;
        private readonly ILogger<AccountController> _logger;
        private readonly IConfiguration _config;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager
            , ITokenService tokenService, IMapper mapper, IEmailSenderService emailService
            , ILogger<AccountController> logger, IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _mapper = mapper;
            _emailService = emailService;
            _logger = logger;
            _config = config;
        }

        [Authorize]
        [HttpGet("getcurrentuser")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var user = await _userManager.FindByEmailFromClaimsPrincipal(User);

            return Ok(new UserDto
            {
                DisplayName = user.DisplayName,
                Token = _tokenService.CreateToken(user),
                Email = user.Email
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
            
            return Ok(_mapper.Map<Address, AddressDto>(user.Address));
        }

        [Authorize]
        [HttpPut("address")]
        public async Task<IActionResult> UpdateUserAddress(AddressDto address)
        {
            var user = await _userManager.FindUserByClaimsPrincipalWithAddress(HttpContext.User);

            user.Address = _mapper.Map<AddressDto, Address>(address);

            var result = await _userManager.UpdateAsync(user);

            if(result.Succeeded) return Ok(user.Address);

            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "problem updating user"));    
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null) return Unauthorized(new ApiResponse(401));

            var isEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
            if (!isEmailConfirmed) return Unauthorized(new ApiResponse(401,"you can not sign in without confirm your email"));

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password
                , false);
            if (!result.Succeeded) return Unauthorized(new ApiResponse(401));

            return Ok(new UserDto
            {
                DisplayName = user.DisplayName,
                Token = _tokenService.CreateToken(user),
                Email = user.Email
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            
            return Ok("user signed out");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
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


            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var confirmationLink = Url.ActionLink("confirmemail", "account", new { email = user.Email, token, Request.Scheme });
            
            _logger.LogInformation(confirmationLink);

            var message = new Message(new List<string> { user.Email}, "BlackStone confirmation email link", confirmationLink);
            await _emailService.SendEmailAsync(message);

            await _userManager.AddToRoleAsync(user, "Visitor");

            
            return Ok("Please check your email for the verification action.");
        }

        [HttpGet("confirmemail")]
        public async Task<IActionResult> ConfirmEmail(string email, string token)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null) return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest));

            var identityResult = await _userManager.ConfirmEmailAsync(user, token);

            if (!identityResult.Succeeded) return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest));

            var removeRoleResult = await _userManager.RemoveFromRoleAsync(user, "Visitor");
            
            if (!removeRoleResult.Succeeded) return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiResponse(StatusCodes.Status500InternalServerError));

            var memberRoleResult = await _userManager.AddToRoleAsync(user, "Member");
            if (!memberRoleResult.Succeeded) return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiResponse(StatusCodes.Status500InternalServerError));

            return Ok($"Thank you for confirming your email.");
        }

        [Authorize]
        [HttpPost("deleteuser")]
        public async Task<IActionResult> DeleteMember(string email)
        {
            var user = await _userManager.FindUserByEmailWithAddress(email);

            if (user == null) return BadRequest(new ApiResponse(400, "user not found"));

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded) return BadRequest(new ApiResponse(400));

            return Ok("user is deleted successfully.");
        }

        [Authorize]
        [HttpPost("changepassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            var user = await _userManager.FindByEmailFromClaimsPrincipal(User);

            var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);

            if (!result.Succeeded) return BadRequest(new ApiValidationErrorResponse()
                    { Errors = result.Errors.Select(e => e.Description) });

            return Ok("Password changed successfully");
        }

        [HttpGet("forgetpassword")]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordDto forgetPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(forgetPasswordDto.Email);

            if (user is null) return BadRequest(new ApiResponse(400, "user not found."));

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var resetpasswordLink = Url.ActionLink("resetpassword", "account", new { email = user.Email, token, Request.Scheme });

            _logger.LogInformation(resetpasswordLink);

            var message = new Message(new List<string> { user.Email }, "BlackStone reset password link", resetpasswordLink);
            await _emailService.SendEmailAsync(message);

            return Ok();
        }

        [HttpPost("resetpassword")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null) return BadRequest(new ApiResponse(400, "user not found"));

            var resetPassResult = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.Password);
            if (!resetPassResult.Succeeded) return BadRequest(new ApiValidationErrorResponse()
            { Errors = resetPassResult.Errors.Select(e => e.Description) });

            return Ok("password reset successfully");
        }

        //[Authorize]
        [HttpGet("getusers")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userManager.Users.Include(u => u.Address).ToListAsync();

            return Ok(_mapper.Map<List<AppUser>, List<UserWithAddressDto>>(users));
        }
    }
}
