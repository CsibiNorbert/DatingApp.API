﻿using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [Route("api/users/{userId}/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IDatingRepository _datingrepo;
        private readonly IMapper _mapper;

        public MessagesController(IDatingRepository datingrepo, IMapper mapper)
        {
            _datingrepo = datingrepo;
            _mapper = mapper;
        }

        [HttpGet("{messageId}", Name ="GetMessage")]
        public async Task<IActionResult> GetMessage(int userId, int messageId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var messageFromRepo = await _datingrepo.GetMessage(messageId);

            if (messageFromRepo == null)
            {
                return NotFound();
            }

            return Ok(messageFromRepo);
        }
        // This is using the route of our controller
        [HttpPost]
        public async Task<IActionResult> CreateMessage(int userId, MessageForCreationDto messageForCreationDto)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            messageForCreationDto.SenderId = userId;

            // The recipientId is sent as part of the body of the request
            var recipient = await _datingrepo.GetUser(messageForCreationDto.RecipientId);

            if (recipient == null)
            {
                return BadRequest("Could not find user");
            }

            var message = _mapper.Map<Message>(messageForCreationDto);

            _datingrepo.Add(message);

            var messageToReturn = _mapper.Map<MessageForCreationDto>(message);

            if (await _datingrepo.SaveAll())
            {
                return CreatedAtRoute("GetMessage", new { messageId = message.Id }, messageToReturn);
            }

            throw new Exception("Creating the message failed on save");
        }
    }
}