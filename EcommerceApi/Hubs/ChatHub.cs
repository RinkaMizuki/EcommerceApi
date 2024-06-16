using EcommerceApi.Dtos.User;
using EcommerceApi.ExtensionExceptions;
using EcommerceApi.Models;
using EcommerceApi.Models.Chat;
using EcommerceApi.Models.UserAddress;
using EcommerceApi.Services;
using EcommerceApi.Services.ChatService;
using EcommerceApi.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace EcommerceApi.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly AdminConnection _adminConnection;
        private readonly IUserService _userService;
        private readonly EcommerceDbContext _context;
        private readonly IConversationService _conversationService;
        private readonly IMessageService _messageService;
        public ChatHub(AdminConnection adminConnection, IUserService userService, EcommerceDbContext context, 
            IConversationService conversationService, IMessageService messageService)
        {
            _adminConnection = adminConnection;
            _userService = userService;
            _context = context;
            _conversationService = conversationService;
            _messageService = messageService;
        }
        public override async Task OnConnectedAsync()
        {
            string roleName = Context
                                    .User
                                    !.Claims
                                    .FirstOrDefault(claim => claim.Type == "Role")
                                    !.Value;
            if (roleName == "admin")
            {
                string adminId = Context
                                        .User
                                        .Claims
                                        .FirstOrDefault(claim => claim.Type == "UserId")
                                        !.Value;
                _adminConnection.AddAdmin(adminId, Context.ConnectionId);
                await SendListParticipant(adminId);
            }
            else
            {
                string userEmail = Context.User.Claims.FirstOrDefault(claim => claim.Type == "Email")!.Value;

                Participation? participant = await _context
                                                        .Participations
                                                        .Where(pp => pp.Email.Equals(userEmail))
                                                        .FirstOrDefaultAsync();
                User receiverAdmin = null;
                string conversationId = string.Empty;
                if (participant is null)
                {
                    KeyValuePair<string, string>? adminRandom = _adminConnection.GetRandomAdmin();
                    if (adminRandom.HasValue)
                    {
                        receiverAdmin = await _userService
                                                        .GetUserByIdAsync(Guid.Parse(adminRandom.Value.Key), CancellationToken.None)
                                                        ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Admin not found.");
                    }
                    else
                    {
                        int count = await _context
                                                 .Users
                                                 .CountAsync(u => u.Role.Equals("admin"));
                        Random random = new();
                        int randomIndex = random.Next(count);

                        receiverAdmin = await _context
                                                    .Users
                                                    .Where(u => u.Role.Equals("admin"))
                                                    .Skip(randomIndex)
                                                    .FirstOrDefaultAsync()
                                                    ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Admin not found.");
                    }
                    var senderId = await _context
                                             .Users
                                             .Where(u => u.Email.Equals(userEmail))
                                             .Select(u => u.UserId)
                                             .FirstOrDefaultAsync(CancellationToken.None);
                                            
                    var conversation = new ConversationDto()
                    {
                        UserId = senderId,
                        Email = userEmail,
                        AdminId = receiverAdmin.UserId,
                    };
                    participant = await _conversationService
                                                            .PostConversationAsync(conversation, CancellationToken.None);
                    conversationId = participant.ConversationId.ToString();
                    await Clients.Users(receiverAdmin.Email).SendAsync("NewParticipant", participant);
                }
                else
                {
                    receiverAdmin = await _userService
                                                     .GetUserByIdAsync(participant.AdminId, CancellationToken.None)
                                                     ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Admin not found.");
                    conversationId = participant.ConversationId.ToString();
                    
                }
                await Clients.Caller.SendAsync("ReceiveAdmin", receiverAdmin, conversationId);
            }
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception ex)
        {
            string adminId = Context
                                    .User
                                    !.Claims
                                    .FirstOrDefault(claim => claim.Type == "UserId")
                                    !.Value;

            if(!string.IsNullOrEmpty(adminId))
            {
                _adminConnection.RemoveAdmin(adminId);
            }
            await base.OnDisconnectedAsync(ex);
        }
        public async Task SendMessageAsync(string senderId, string email, string message, string conversationId, string? originMessageId)
        {
            var messageDto = new MessageDto()
            {
                MessageId = Guid.NewGuid(),
                ConversationId = Guid.Parse(conversationId),
                MessageContent = message,
                OriginalMessageId = Guid.TryParse(originMessageId, out Guid id) ? id : null,
                SenderId = Guid.Parse(senderId)
            };

            string adminEmail = Context.User.Claims.FirstOrDefault(claim => claim.Type == "Email")!.Value;

            var newMessage = await _messageService
                                            .PostMessageAsync(messageDto, CancellationToken.None);
            await Clients
                        .Users(email, adminEmail)
                        .SendAsync("ReceiveMessage", newMessage);
        }
        public async Task GetMessageAsync(Guid conversationId)
        {
            var listMessage = await _messageService.GetListMessageAsync(conversationId, CancellationToken.None);
            await Clients.Caller.SendAsync("ReceiveMessages", listMessage);
        }
        private async Task SendListParticipant(string adminId)
        {
            var participentList = await _conversationService.GetListParticipationAsync(Guid.Parse(adminId), CancellationToken.None);
            await Clients.Caller.SendAsync("ReceiveParticipants", participentList);
        }
    }
}
