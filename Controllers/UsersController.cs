using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository _datingrepo;
        private readonly IMapper _mapper;

        public UsersController(IDatingRepository datingrepo, IMapper mapper)
        {
            _datingrepo = datingrepo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _datingrepo.GetUsers();

            // IEnumerable because we return a list of users
            var usersToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);

            return Ok(usersToReturn);
        }

        [HttpGet("{id}", Name = "GetUser")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _datingrepo.GetUser(id);

            // Destination => user = source
            // In order this automapper to work, it needs a profile
            var userToReturn = _mapper.Map<UserForDetailedDto>(user);

            return Ok(userToReturn);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userForUpdateDto)
        {
            // is matching the token for that particular user?
            // Compare id of the path, with the user id from the token
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            // Retrieve user
            var userFromRepo = await _datingrepo.GetUser(id);

            // Map user with Dto
            // It will take dto => user
            _mapper.Map(userForUpdateDto,userFromRepo);

            if (await _datingrepo.SaveAll())
            {
                return NoContent();
            }

            // If we reach here, something went wrong
            throw new Exception($"Updating user with { id } failed on save");
        }
    }
}
