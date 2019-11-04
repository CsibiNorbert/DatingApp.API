using DatingApp.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository _datingrepo;

        public UsersController(IDatingRepository datingrepo)
        {
            _datingrepo = datingrepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _datingrepo.GetUsers();

            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _datingrepo.GetUser(id);

            return Ok(user);
        }
    }
}
