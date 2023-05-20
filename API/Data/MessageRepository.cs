using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public MessageRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;            
        }
        
        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FindAsync(id);
        }

        public async Task<PagedList<MessageDTO>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = _context.Messages.OrderByDescending(x => x.MessageSent).AsQueryable();            
            query = messageParams.Container switch
            {
                "Inbox"  => query.Where(m => m.RecipientName == messageParams.UserName && !m.RecipientDeleted),
                "Outbox" => query.Where(m => m.SenderName == messageParams.UserName && !m.SenderDeleted),
                    _    => query.Where(m => m.RecipientName == messageParams.UserName && !m.RecipientDeleted
                            && m.DateRead == null)
            };

            var messages = query.ProjectTo<MessageDTO>(_mapper.ConfigurationProvider);

            return await PagedList<MessageDTO>
                .CreateAsync(messages, messageParams.PageNumber,messageParams.PageSize);

        }

        public async Task<IEnumerable<MessageDTO>> GetMessageThread(string currentUserName, string recipientUserName)
        {
        var messages = await _context.Messages
                        .Include(u => u.Sender).ThenInclude(p=>p.Photos)
                        .Include(u => u.Recipient).ThenInclude(p=>p.Photos)
                        .Where(x => x.SenderName == currentUserName && !x.SenderDeleted && x.RecipientName == recipientUserName 
                         || x.RecipientName == currentUserName && !x.RecipientDeleted && x.SenderName == recipientUserName)
                        .OrderByDescending(m => m.MessageSent)
                        .ToListAsync();

        var unreadMessages = messages
                            .Where(m=> m.DateRead == null && m.RecipientName == currentUserName).ToList();

        if(unreadMessages.Any())
        {
            foreach(var message in unreadMessages)
            {
                message.DateRead = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
        }

        return _mapper.Map<IEnumerable<MessageDTO>>(messages);
                        
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }      

    }
}