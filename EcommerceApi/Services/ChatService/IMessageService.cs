using EcommerceApi.Dtos.User;
using EcommerceApi.Models.Chat;

namespace EcommerceApi.Services.ChatService
{
    public interface IMessageService
    {
        public Task<Message> PostMessageAsync(MessageDto messageDto, CancellationToken cancellationToken);
        public Task<bool> DeleteMessageAsync(Guid messageId, CancellationToken cancellationToken);
        public Task<List<Message>> GetListMessageAsync(Guid conversationId, CancellationToken cancellationToken);
        public Task<bool> UpdateMessageAsync(MessageDto messageDto, CancellationToken cancellationToken);
        public Task<Message> GetMessageByIdAsync(Guid messageId, CancellationToken cancellationToken);
    }
}
