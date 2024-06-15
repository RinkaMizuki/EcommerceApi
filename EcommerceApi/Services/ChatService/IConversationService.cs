using EcommerceApi.Dtos.User;

namespace EcommerceApi.Services.ChatService
{
    public interface IConversationService
    {
        public Task<bool> PostConversationAsync(ConversationDto conversationDto, CancellationToken cancellationToken);
        public Task<bool> DeleteConversationAsync(Guid ConversationId, CancellationToken cancellationToken);
        //public Task<>
    }
}
