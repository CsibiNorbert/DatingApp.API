using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DatingApp.API.Controllers
{
    // Any time when a method is being called we make use of user activity action filer which will update the last active prop
    [ServiceFilter(typeof(LogUserActivity))]
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

        // FromQuery will allow us to send an empty query string
        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] UserParams userParams)
        {
            //Getting current user id from the token
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userFromRepo = await _datingrepo.GetUser(currentUserId);

            userParams.UserId = userFromRepo.Id;
            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = userFromRepo.Gender == "male" ? "female" : "male";
            }

            var users = await _datingrepo.GetUsers(userParams);

            // IEnumerable because we return a list of users
            var usersToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);

            // We pass this information back in the header
            Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

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
            _mapper.Map(userForUpdateDto, userFromRepo);

            if (await _datingrepo.SaveAll())
            {
                return NoContent();
            }

            // If we reach here, something went wrong
            throw new Exception($"Updating user with { id } failed on save");
        }

        [HttpPost("{id}/like/{recipientId}")]
        public async Task<IActionResult> LikeUser(int id, int recipientId)
        {
            // is matching the token for that particular user?
            // Compare id of the path, with the user id from the token
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var like = await _datingrepo.GetLike(id, recipientId);

            if (like != null)
            {
                return BadRequest("You already liked this user");
            }

            if (await _datingrepo.GetUser(recipientId) == null)
            {
                return NotFound();
            }

            like = new Like
            {
                LikerId = id,
                LikeeId = recipientId
            };

            // Is adding this into memory, si not saving it yet
            _datingrepo.Add<Like>(like);

            if (await _datingrepo.SaveAll())
            {
                return Ok();
            }

            // if we don`t save successfully we just return bad request
            return BadRequest("Failed to like user");
        }
    }
}