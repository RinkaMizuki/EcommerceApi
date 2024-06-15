using EcommerceApi.Dtos.User;
using EcommerceApi.ExtensionExceptions;
using EcommerceApi.Models;
using EcommerceApi.Models.Chat;
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

        public Task<List<Message>> GetListMessageAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Message> GetMessageByIdAsync(Guid messageId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> PostMessageAsync(MessageDto messageDto, CancellationToken cancellationToken)
        {
            try
            {
                var newMessage = new Message()
                {
                    MessageId = Guid.NewGuid(),
                    MessageContent = messageDto.MessageContent,
                    SeenderId = messageDto.SeenderId,
                    ModifiedAt = DateTime.UtcNow,
                    SendAt = DateTime.UtcNow,
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
                return true;
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
