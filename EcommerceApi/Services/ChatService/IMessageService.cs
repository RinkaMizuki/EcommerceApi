using EcommerceApi.Dtos.User;
using EcommerceApi.Models.Chat;

namespace EcommerceApi.Services.ChatService
{
    public interface IMessageService
    {
        public Task<bool> PostMessageAsync(MessageDto messageDto, CancellationToken cancellationToken);
        public Task<bool> DeleteMessageAsync(Guid messageId, CancellationToken cancellationToken);
        public Task<List<Message>> GetListMessageAsync(CancellationToken cancellationToken);
        public Task<bool> UpdateMessageAsync(MessageDto messageDto, CancellationToken cancellationToken);
        public Task<Message> GetMessageByIdAsync(Guid messageId, CancellationToken cancellationToken);
    }
}
