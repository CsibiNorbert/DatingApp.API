using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        // Injecting the IAuthRepository to the controller.
        public AuthController(IAuthRepository authRepository,
                              IConfiguration configuration,
                              IMapper mapper,
                              UserManager<User> userManager,
                              SignInManager<User> signInManager)
        {
            _authRepository = authRepository;
            _configuration = configuration;
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserToBeRegisteredDto userToBeRegistered)
        {
            userToBeRegistered.Username = userToBeRegistered.Username.ToLower();

            if (await _authRepository.UserExists(userToBeRegistered.Username))
            {
                return BadRequest("Username already exists");
            }

            var userToCreate = _mapper.Map<User>(userToBeRegistered);

            var createdUser = await _authRepository.Register(userToCreate, userToBeRegistered.Password);

            var userToReturn = _mapper.Map<UserForDetailedDto>(createdUser);

            // This GetUser is the name of the route in the user controller
            // The location in the headers points now to this API
            return CreatedAtRoute("GetUser", new { controller = "Users", id = createdUser.Id }, userToReturn);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserToLoginDto userToLoginDto)
        {
            // provide the credentials to the Login method and if user doesn`t exist, then we return null.
            //var userFromRepo = await _authRepository.Login(userToLoginDto.Username.ToLower(), userToLoginDto.Password);

            var user = await _userManager.FindByNameAsync(userToLoginDto.Username);

            // Will get the user found by the username and it will compare the password, and then we say false as we dont need to lock out our user.
            var result = await _signInManager.CheckPasswordSignInAsync(user, userToLoginDto.Password, false);

            // If null return unauthorized
            if (result.Succeeded)
            {
                // This is used to retrieve the main photo, we mapp to UserForListDto
                var appUser = _mapper.Map<UserForListDto>(user);

                return Ok(
                    new
                    {
                        token = GenerateJwtToken(user),
                        user = appUser // this is what we return, so that we dont return the excesive information
                    });
            }

            return Unauthorized();            
        }

        private string GenerateJwtToken(User user)
        {
            #region Build Token
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new Claim(ClaimTypes.Name,user.UserName)
            };

            // This is created in appsettings json
            // This should be a long string of randomly generated characters when goes to prod
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));

            // we use the above key to add a signing
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            // Token is created from here down
            // Will contain our claims, expiry date and the signing credentials
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1), // will expire in a date`s time
                SigningCredentials = credentials
            };

            // we need the token handler in order to create a token so that we pass the token descriptor
            var tokenhandler = new JwtSecurityTokenHandler();

            var token = tokenhandler.CreateToken(tokenDescriptor);

            #endregion Build Token

            return tokenhandler.WriteToken(token);
        }
    }
}