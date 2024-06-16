using EcommerceApi.Dtos.User;
using EcommerceApi.ExtensionExceptions;
using EcommerceApi.Models;
using EcommerceApi.Models.Chat;
using Microsoft.EntityFrameworkCore;
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
        public async Task<bool> DeleteConversationAsync(Guid ConversationId, CancellationToken cancellationToken)
        {
            try
            {
                var conversationDeleteed = await _context
                                                      .Conversations
                                                      .Where(cs => cs.ConversationId.Equals(ConversationId))
                                                      .FirstOrDefaultAsync(cancellationToken)
                                                      ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Conversation not found.");
                _context
                    .Conversations
                    .Remove(conversationDeleteed);
                await _context
                    .SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        public async Task<List<Participation>> GetListParticipationAsync(Guid adminId, CancellationToken cancellationToken)
        {
            try
            {
                var participentList = await _context
                                                    .Participations
                                                    .Include(pp => pp.User)
                                                    .Include(pp => pp.Conversation)
                                                    .Where(pp => pp.AdminId.Equals(adminId))
                                                    .AsNoTracking()
                                                    .ToListAsync(cancellationToken);
                return participentList;
            }
            catch(Exception ex) 
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<Participation> PostConversationAsync(ConversationDto conversationDto, CancellationToken cancellationToken)
        {
            try
            {
                // Lấy thông tin người dùng từ cơ sở dữ liệu
                var user = await _context.Users
                                         .FindAsync(new object[] { conversationDto.UserId }, cancellationToken);

                if (user == null)
                {
                    throw new HttpStatusException(HttpStatusCode.NotFound, "User not found");
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
                    User = user,
                    AdminId = conversationDto.AdminId,
                };
                await _context
                            .Conversations
                            .AddAsync(newConversation, cancellationToken);
                await _context
                            .Participations
                            .AddAsync(newParticipation, cancellationToken);
                await _context
                            .SaveChangesAsync(cancellationToken);
                return newParticipation;
            }
            catch (Exception ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
