﻿using AutoMapper;
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
    [ServiceFilter(typeof(LogUserActivity))]
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

        [HttpGet("{messageId}", Name = "GetMessage")]
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

        // This is not conflicting with the HttpGet for a sp[ecific message.
        // This will get a list of messages
        [HttpGet]
        public async Task<IActionResult> GetMessagesForUser(int userId, [FromQuery] MessageParams messageParams)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            messageParams.UserId = userId;

            var messagesFromrepo = await _datingrepo.GetMessagesForUser(messageParams);

            var messages = _mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromrepo);

            Response.AddPagination(messagesFromrepo.CurrentPage, messagesFromrepo.PageSize, messagesFromrepo.TotalCount, messagesFromrepo.TotalPages);

            return Ok(messages);
        }

        [HttpGet("thread/{recipientId}")]
        public async Task<IActionResult> GetMessageThread(int userId, int recipientId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var messagesFromRepo = await _datingrepo.GetMessageThread(userId, recipientId);

            var messageThread = _mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromRepo);

            return Ok(messageThread);
        }

        // This is using the route of our controller
        [HttpPost]
        public async Task<IActionResult> CreateMessage(int userId, MessageForCreationDto messageForCreationDto)
        {
            var sender = await _datingrepo.GetUser(userId,false);

            if (sender.Id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            messageForCreationDto.SenderId = userId;

            // The recipientId is sent as part of the body of the request
            var recipient = await _datingrepo.GetUser(messageForCreationDto.RecipientId,false);

            if (recipient == null)
            {
                return BadRequest("Could not find user");
            }

            var message = _mapper.Map<Message>(messageForCreationDto);

            _datingrepo.Add(message);

            if (await _datingrepo.SaveAll())
            {
                var messageToReturn = _mapper.Map<MessageToReturnDto>(message);
                return CreatedAtRoute("GetMessage", new { userId, messageId = message.Id }, messageToReturn);
            }

            throw new Exception("Creating the message failed on save");
        }

        // We don`t want to put an HttpDelete, because we want the message to be deleted only if both sides delete the message
        [HttpPost("{messageId}")]
        public async Task<IActionResult> DeleteMessage(int messageId, int userId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var messageFromRepo = await _datingrepo.GetMessage(messageId);

            // Check is the message id has the logged in user id and delete it
            if (messageFromRepo.SenderId == userId)
            {
                messageFromRepo.SenderDeleted = true;
            }

            if (messageFromRepo.RecipientId == userId)
            {
                messageFromRepo.RecipientDeleted = true;
            }

            // Delete completely the message if both sides have deleted the message on their side
            if (messageFromRepo.RecipientDeleted && messageFromRepo.RecipientDeleted)
            {
                _datingrepo.Delete(messageFromRepo);
            }

            if (await _datingrepo.SaveAll())
            {
                // Dont return anything to the person which is deleting this message
                return NoContent();
            }

            throw new Exception("Error deleting the message");
        }

        [HttpPost("{messageId}/read")]
        public async Task<IActionResult> MarkMessageAsRead(int userId, int messageId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var message = await _datingrepo.GetMessage(messageId);

            if (message.RecipientId != userId)
            {
                return Unauthorized();
            }

            message.IsRead = true;
            message.DateRead = DateTime.Now;

            await _datingrepo.SaveAll();

            return NoContent();
        }
    }
}