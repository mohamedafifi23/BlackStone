using API.Dtos;
using API.Extensions;
using AutoMapper;
using Core.Entities.Identity;
using Core.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager
            , ITokenService tokenService, IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _mapper = mapper;
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
            return Ok(await _userManager.FindByEmailAsync(email) != null);
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

            return BadRequest("problem updating user");    
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null) return Unauthorized();

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password
                , false);

            if (!result.Succeeded) return Unauthorized();

            return Ok(new UserDto
            {
                DisplayName = user.DisplayName,
                Token = _tokenService.CreateToken(user),
                Email = user.Email
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            //check email exists

            var user = new AppUser()
            {
                Email = registerDto.Email,
                DisplayName = registerDto.DisplayName,
                UserName=registerDto.Email
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded) return BadRequest();

            await _userManager.AddToRoleAsync(user, "Member");

            return Ok(new UserDto
            {
                DisplayName=user.DisplayName,
                Token=_tokenService.CreateToken(user),
                Email= user.Email
            });
        } 
    }
}
