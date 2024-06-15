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
        public ChatHub(AdminConnection adminConnection, IUserService userService, EcommerceDbContext context, 
            IConversationService conversationService)
        {
            _adminConnection = adminConnection;
            _userService = userService;
            _context = context;
            _conversationService = conversationService;
        }
        public override async Task OnConnectedAsync()
        {
            string roleName = Context.User.Claims.FirstOrDefault(claim => claim.Type == "Role")!.Value;
            if (roleName == "admin")
            {
                string adminId = Context.User.Claims.FirstOrDefault(claim => claim.Type == "UserId")!.Value;
                _adminConnection.AddAdmin(adminId, Context.ConnectionId);
            }
            else
            {
                string userEmail = Context.User.Claims.FirstOrDefault(claim => claim.Type == "Email")!.Value;

                Participation? participant = await _context
                                                        .Participations
                                                        .Where(pp => pp.Email.Equals(userEmail))
                                                        .FirstOrDefaultAsync();
                User receiverAdmin = null;
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
                    var conversation = new ConversationDto()
                    {
                        UserId = receiverAdmin.UserId,
                        Email = userEmail,
                    };
                    await _conversationService
                                            .PostConversationAsync(conversation, CancellationToken.None);
                }
                else
                {
                    receiverAdmin = await _userService
                                                     .GetUserByIdAsync(participant.UserId, CancellationToken.None)
                                                     ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Admin not found.");
                }

                await Clients.Caller.SendAsync("ReceiveAdmin", receiverAdmin);
            }
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception ex)
        {
            string adminId = Context.User.Claims.FirstOrDefault(claim => claim.Type == "UserId")!.Value;
            if(!string.IsNullOrEmpty(adminId))
            {
                _adminConnection.RemoveAdmin(adminId);
            }
            await base.OnDisconnectedAsync(ex);
        }
        public async Task SendMessageAsync(string userId, string message)
        {
            await Clients.User(userId).SendAsync("ReceiveMessage", message);
        }
    }
}
