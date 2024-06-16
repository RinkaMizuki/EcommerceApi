using EcommerceApi.Dtos.User;
using EcommerceApi.Models.Chat;

namespace EcommerceApi.Services.ChatService
{
    public interface IConversationService
    {
        public Task<Participation> PostConversationAsync(ConversationDto conversationDto, CancellationToken cancellationToken);
        public Task<bool> DeleteConversationAsync(Guid ConversationId, CancellationToken cancellationToken);
        public Task<List<Participation>> GetListParticipationAsync(Guid adminId, CancellationToken cancellationToken);
    }
}
