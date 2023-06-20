using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    [Authorize]
    public class MessageHub : Hub
    {
        private readonly IMapper _mapper;
        private readonly IMessageRepository _messageRepository;
        private readonly IUserRepository _userRepository;
        private readonly IHubContext<PresenceHub> _presenceHub;

        public MessageHub(IMessageRepository messageRepository, 
        IUserRepository userRepository, IMapper mapper, 
        IHubContext<PresenceHub> presenceHub)
        {
            _presenceHub = presenceHub;
            _userRepository = userRepository;
            _messageRepository = messageRepository;
            _mapper = mapper;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var other = httpContext.Request.Query["user"];
            var groupName = this.GetGroupName(Context.User.GetUserName(), other);
            
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            
            var group = AddToGroup(groupName);
            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

            var messages = await _messageRepository.GetMessageThread(Context.User.GetUserName(), other);
            await Clients.Caller.SendAsync("RecieveMessageThread", messages);
        }

        public string GetGroupName(string caller, string other)
        {
            var strCmpr =  string.CompareOrdinal(caller,other)<0;
            return strCmpr ? $"{caller}-{other}" : $"{other}-{caller}";
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var group = await RemoveFromMessageGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessageAsync(CreateMessageDTO createMessageDto)
        {
        var userName = Context.User.GetUserName();

        if(userName == createMessageDto.RecipientUserName)
        throw new HubException("You cannot send message to yourself!");

        var sender = await _userRepository.GetUserByUserNameAsync(userName);

        var recipient = await _userRepository.GetUserByUserNameAsync(createMessageDto.RecipientUserName);

        if(recipient == null) throw new HubException("Not found the user!");

        var message = new Message{
            Sender = sender,
            Recipient = recipient,
            SenderName = sender.UserName,
            RecipientName = recipient.UserName,
            Content = createMessageDto.Content
        };

        var groupName = GetGroupName(sender.UserName, recipient.UserName);
        var group = await _messageRepository.GetMessageGroup(groupName);

        if(group.Connections.Any(x=>x.UserName == recipient.UserName))
        {
            message.DateRead = DateTime.UtcNow;
        }
        else
        {
            var connections = await PresenceTracker.GetConnectionsForUser(recipient.UserName);
            if(connections != null)
            {
                await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived",
                    new {username = sender.UserName, knownas = sender.KnownAs});
            }
        }

        _messageRepository.AddMessage(message);

        if(await _messageRepository.SaveAllAsync()) 
        {
            await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDTO>(message));
        }
                    
        }

        private async Task<Group> AddToGroup(string groupName)
        {
            var group = await _messageRepository.GetMessageGroup(groupName);
            var connection = new Connection(Context.ConnectionId, Context.User.GetUserName());
        
        if(group==null)
        {
            group = new Group(groupName);
            _messageRepository.AddGroup(group);
        }

        group.Connections.Add(connection);
        if( await _messageRepository.SaveAllAsync())
            return group;

        throw new HubException("Failed to add to the group");
        }

        private async Task<Group> RemoveFromMessageGroup()
        {
            var group = await _messageRepository.GetGroupForConnection(Context.ConnectionId);
            var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            _messageRepository.RemoveConnection(connection);
            
            if(await _messageRepository.SaveAllAsync())
               return group; 
        
            throw new HubException("Failed to remove from the group");
        }
    }
}