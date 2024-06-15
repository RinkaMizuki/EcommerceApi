using EcommerceApi.Dtos.User;
using EcommerceApi.ExtensionExceptions;
using EcommerceApi.Models;
using EcommerceApi.Models.Chat;
using System.Net;

namespace EcommerceApi.Services.ChatService
{
    public class ConversationService : IConversationService
    {
        private readonly EcommerceDbContext _context;
        public ConversationService(EcommerceDbContext context)
        {
            _context = context;
        }
        public Task<bool> DeleteConversationAsync(Guid ConversationId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> PostConversationAsync(ConversationDto conversationDto, CancellationToken cancellationToken)
        {
            try
            { 
                var isJoined = _context.Participations.Any(pp => pp.UserId.Equals(conversationDto.UserId));
                if(isJoined)
                {
                    return false;
                }
                var newConversation = new Conversation()
                {
                    ConversationId = Guid.NewGuid(),
                    Title = conversationDto.Title,
                    StartedAt = DateTime.UtcNow,
                    LastestMessage = conversationDto.LastestMessage,
                    LastestSend = DateTime.UtcNow,
                };
                var newParticipation = new Participation()
                {
                    ConversationId = newConversation.ConversationId,
                    JoinedAt = DateTime.UtcNow,
                    UserId = conversationDto.UserId,
                    Email = conversationDto.Email,
                };
                await _context
                            .Conversations
                            .AddAsync(newConversation, cancellationToken);
                await _context
                            .Participations
                            .AddAsync(newParticipation, cancellationToken);
                await _context
                            .SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
