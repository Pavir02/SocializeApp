using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class MessagesController : BaseApiController
    {        
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

      public MessagesController(IMapper mapper, IUnitOfWork uow)
      {
            _uow = uow;
            _mapper = mapper;
      }  

      [HttpPost]
      public async Task<ActionResult<MessageDTO>> CreateMessage(CreateMessageDTO createMessageDto)
      {
        var userName = User.GetUserName();
        if(userName == createMessageDto.RecipientUserName)
        return BadRequest("You cannot send message to yourself!");

        var sender = await _uow.UserRepository.GetUserByUserNameAsync(userName);

        var recipient = await _uow.UserRepository.GetUserByUserNameAsync(createMessageDto.RecipientUserName);

        if(recipient == null) return NotFound();

        var message = new Message{
            Sender = sender,
            Recipient = recipient,
            SenderName = sender.UserName,
            RecipientName = recipient.UserName,
            Content = createMessageDto.Content
        };

        _uow.MessageRepository.AddMessage(message);

        if(await _uow.Complete()) 
            return Ok(_mapper.Map<MessageDTO>(message));

        return BadRequest("Failed to send Message!");
      }

      [HttpGet]
      public async Task<ActionResult<PagedList<MessageDTO>>> GetMessagesForUser([FromQuery] 
      MessageParams messageParams)
      {
        messageParams.UserName = User.GetUserName();

        var messages = await _uow.MessageRepository.GetMessagesForUser(messageParams);

        Response.AddPaginationHeader(new PaginationHeader(messages.TotalCount, 
            messages.CurrentPage,messages.PageSize, messages.TotalPages));

        return messages;
      }

      // [HttpGet("thread/{username}")]
      // public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMessageThread(string userName)
      // {
      //   var currentUserName = User.GetUserName();
      //   return Ok(await _uow.MessageRepository.GetMessageThread(currentUserName, userName));
      // }

      [HttpDelete("{id}")]
      public async Task<ActionResult> DeleteMessage(int id)
      {
        var username = User.GetUserName();
        var message = await _uow.MessageRepository.GetMessage(id);

        if(message.SenderName != username && message.RecipientName !=username) 
          return Unauthorized();

        if(message.SenderName == username) message.SenderDeleted = true;
        if(message.RecipientName == username) message.RecipientDeleted = true;

        if(message.SenderDeleted && message.RecipientDeleted) 
          _uow.MessageRepository.DeleteMessage(message);

          if(await _uow.Complete()) 
            return Ok();
        
          return BadRequest("Problem deleting message!");
      }
    }
}