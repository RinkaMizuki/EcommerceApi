using EcommerceApi.Dtos.User;
using EcommerceApi.Models.Chat;
using EcommerceApi.Responses;

namespace EcommerceApi.Services.ChatService
{
    public interface IConversationService
    {
        public Task<Participation> PostConversationAsync(ConversationDto conversationDto, CancellationToken cancellationToken);
        public Task<bool> DeleteConversationAsync(Guid conversationId, CancellationToken cancellationToken);
        public Task<List<ParticipationResponse>> GetListParticipationAsync(string? filter, CancellationToken cancellationToken);
        public Task<ConversationResponse?> GetConversationByIdAsync(Guid conversationId, CancellationToken cancellationToken);
    }
}
