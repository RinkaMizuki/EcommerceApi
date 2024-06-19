using EcommerceApi.Dtos.User;
using EcommerceApi.ExtensionExceptions;
using EcommerceApi.Models;
using EcommerceApi.Models.Chat;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace EcommerceApi.Services.ChatService
{
    public class MessageService : IMessageService
    {
        private readonly EcommerceDbContext _context;
        public MessageService(EcommerceDbContext context)
        {
            _context = context;
        }
        public Task<bool> DeleteMessageAsync(Guid messageId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Message>> GetListMessageAsync(Guid conversationId, CancellationToken cancellationToken)
        {
            try
            {
                var listMessage = await _context
                                                .Messages
                                                .Where(m => m.ConversationId == conversationId)
                                                .Include(m => m.Sender)
                                                .ToListAsync(cancellationToken);
                return listMessage;
            }
            catch(Exception ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public Task<Message> GetMessageByIdAsync(Guid messageId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<Message> PostMessageAsync(MessageDto messageDto, CancellationToken cancellationToken)
        {
            try
            {
                var newMessage = new Message()
                {
                    MessageId = (Guid)(messageDto.MessageId != null ? messageDto.MessageId : Guid.NewGuid()),
                    MessageContent = messageDto.MessageContent,
                    SenderId = messageDto.SenderId,
                    ModifiedAt = DateTime.UtcNow,
                    SendAt = DateTime.UtcNow,
                    ConversationId = messageDto.ConversationId,
                };

                if (messageDto.OriginalMessageId is not null)
                {
                    newMessage.OriginalMessageId = messageDto.OriginalMessageId;
                }

                await _context
                            .Messages
                            .AddAsync(newMessage, cancellationToken);

                await _context
                            .SaveChangesAsync(cancellationToken);
                return await _context
                                    .Messages
                                    .Where(m => m.MessageId.Equals(newMessage.MessageId))
                                    .Include(m => m.Sender)
                                    .FirstAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public Task<bool> UpdateMessageAsync(MessageDto messageDto, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
