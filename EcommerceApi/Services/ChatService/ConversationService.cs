using EcommerceApi.Constant;
using EcommerceApi.Dtos.User;
using EcommerceApi.ExtensionExceptions;
using EcommerceApi.FilterBuilder;
using EcommerceApi.Models;
using EcommerceApi.Models.Chat;
using EcommerceApi.Responses;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace EcommerceApi.Services.ChatService
{
    public class ConversationService : IConversationService
    {
        private readonly ChatFilterBuilder _chatFilter;
        private readonly EcommerceDbContext _context;
        public ConversationService(EcommerceDbContext context, ChatFilterBuilder chatFilterBuilder)
        {
            _context = context;
            _chatFilter = chatFilterBuilder;
        }
        public async Task<bool> DeleteConversationAsync(Guid conversationId, CancellationToken cancellationToken)
        {
            try
            {
                var conversationDeleteed = await _context
                                                      .Conversations
                                                      .Where(cs => cs.ConversationId.Equals(conversationId))
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

        public async Task<ConversationResponse?> GetConversationByIdAsync(Guid conversationId, CancellationToken cancellationToken)
        {
            try
            {
                var conversation = await _context
                                                .Conversations
                                                .Where(cs => cs.ConversationId.Equals(conversationId))
                                                .Include(cs => cs.Messages)
                                                .ThenInclude(m => m.Sender)
                                                .Select(cs => new ConversationResponse
                                                {
                                                    ConversationId = cs.ConversationId,
                                                    IsSeen = cs.IsSeen,
                                                    StartedAt = cs.StartedAt,
                                                    Title = cs.Title,
                                                    LastMessage = cs.Messages
                                                                            .OrderByDescending(m => m.SendAt)
                                                                            .First()
                                                })
                                                .FirstOrDefaultAsync(cancellationToken);
                return conversation;
            }
            catch (Exception ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<List<ParticipationResponse>> GetListParticipationAsync(string? filter, CancellationToken cancellationToken)
        {
            try
            {
                var filterValues = Helpers.ParseString<string>(filter);

                if(!filterValues.Contains(ChatFilterType.Search))
                {
                    filterValues.Add(ChatFilterType.Search);
                    filterValues.Add("");
                }

                if (!filterValues.Contains(ChatFilterType.Id))
                {
                    filterValues.Add(ChatFilterType.Id);
                    filterValues.Add("");
                }

                var searchValue = filterValues[filterValues.IndexOf(ChatFilterType.Search) + 1].ToLower();
                var adminId = filterValues[filterValues.IndexOf(ChatFilterType.Id) + 1];

                var filters = _chatFilter
                                        .AddSearchFilter(searchValue)
                                        .AddIdFilter(adminId)
                                        .Build();

                var participentList =  _context
                                               .Participations
                                               .Include(pp => pp.User)
                                               .Include(pp => pp.Conversation)
                                               .ThenInclude(cs => cs.Messages)
                                               .ThenInclude(msg => msg.Sender)
                                               .AsNoTracking()
                                               .Where(filters)
                                               .Select(pp => new ParticipationResponse
                                                    {
                                                        ConversationId = pp.ConversationId,
                                                        Conversation = new ConversationResponse
                                                        {
                                                            ConversationId = pp.Conversation.ConversationId,
                                                            IsSeen = pp.Conversation.IsSeen,
                                                            LastMessage = pp.Conversation.Messages
                                                                                        .OrderByDescending(m => m.SendAt)
                                                                                        .First(),
                                                            StartedAt = pp.Conversation.StartedAt,
                                                            Title = pp.Conversation.Title,
                                                        },
                                                        AdminId = pp.AdminId,
                                                        JoinedAt = pp.JoinedAt,
                                                        User = pp.User,
                                                        UserId = pp.UserId,
                                                    })
                                               .AsQueryable();
                return participentList.ToList();
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
                };
                var newParticipation = new Participation()
                {
                    ConversationId = newConversation.ConversationId,
                    JoinedAt = DateTime.UtcNow,
                    UserId = conversationDto.UserId,
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
