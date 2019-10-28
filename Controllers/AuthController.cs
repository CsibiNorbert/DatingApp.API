using DatingApp.API.Data;
using DatingApp.API.Models;
using DatingApp.API.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;

        // Injecting the IAuthRepository to the controller.
        public AuthController(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserToBeRegisteredDto userToBeRegistered)
        {

            userToBeRegistered.Username = userToBeRegistered.Username.ToLower();

            if (await _authRepository.UserExists(userToBeRegistered.Username))
            {
                return BadRequest("Username already exists");
            }

            var userToCreate = new User
            {
                Username = userToBeRegistered.Username
            };

            var createdUder = await _authRepository.Register(userToCreate, userToBeRegistered.Password);

            return StatusCode(201);
        }
    }
}
