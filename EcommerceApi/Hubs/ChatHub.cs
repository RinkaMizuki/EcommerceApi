﻿using EcommerceApi.Constant;
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
using Newtonsoft.Json;
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
        private readonly UserConnection _userConnection;
        public ChatHub(AdminConnection adminConnection, IUserService userService, EcommerceDbContext context, 
            IConversationService conversationService, IMessageService messageService, UserConnection userConnection)
        {
            _adminConnection = adminConnection;
            _userService = userService;
            _context = context;
            _conversationService = conversationService;
            _messageService = messageService;
            _userConnection = userConnection;

        }
        public override async Task OnConnectedAsync()
        {
            string roleName = Context
                                    .User
                                    !.Claims
                                    .FirstOrDefault(claim => claim.Type == "Role")
                                    ?.Value ?? "member";
            string id = Context.User.Claims.FirstOrDefault(claim => claim.Type == "UserId")!.Value;

            if (roleName == "admin")
            {
                _adminConnection.AddAdmin(id, Context.ConnectionId);

                await SendListParticipant(id);
      
                await Clients
                            .Users(_userConnection.GetAllUser())
                            .SendAsync("ReceiveStatus", id, ChatStatus.Connected);
                await Clients
                            .User(id)
                            .SendAsync("ReceiveListUserStatus", _userConnection.GetAllUser());
            }
            else
            {
                string userId = Context.User.Claims.FirstOrDefault(claim => claim.Type == "UserId")!.Value;

                Participation? participant = await _context
                                                        .Participations
                                                        .Where(pp => pp.UserId.Equals(Guid.Parse(userId)))
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
                    var conversation = new ConversationDto()
                    {
                        UserId = Guid.Parse(userId),
                        AdminId = receiverAdmin.UserId,
                    };
                    participant = await _conversationService
                                                            .PostConversationAsync(conversation, CancellationToken.None);
                    conversationId = participant.ConversationId.ToString();
                    await Clients
                                .Users(receiverAdmin.UserId.ToString())
                                .SendAsync("ReceiveNewParticipant", participant);
                }
                else
                {
                    receiverAdmin = await _userService
                                                     .GetUserByIdAsync(participant.AdminId, CancellationToken.None)
                                                     ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Admin not found.");
                    conversationId = participant.ConversationId.ToString();
                    
                }
                _userConnection.AddUser(userId, receiverAdmin.UserId.ToString());

                await Clients
                            .Caller
                            .SendAsync("ReceiveAdmin", receiverAdmin, conversationId);
                await Clients
                            .User(receiverAdmin.UserId.ToString())
                            .SendAsync("ReceiveListUserStatus", _userConnection.GetAllUser());
                await Clients
                            .User(userId)
                            .SendAsync("ReceiveStatus", receiverAdmin.UserId, _adminConnection.IsOnlineAdmin(receiverAdmin.UserId.ToString()));
            }
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception ex)
        {
            string roleName = Context
                                  .User
                                  !.Claims
                                  .FirstOrDefault(claim => claim.Type == "Role")
                                  ?.Value ?? "member";
            string id = Context
                              .User
                              !.Claims
                              .FirstOrDefault(claim => claim.Type == "UserId")
                              !.Value;

            if (!string.IsNullOrEmpty(roleName))
            {
                if(roleName == "admin")
                {
                    _adminConnection.RemoveAdmin(id);

                    await Clients
                                .Users(_userConnection.GetAllUser())
                                .SendAsync("ReceiveStatus", id, ChatStatus.Disconnected);
                }
                else
                {
                    var adminId = _userConnection.GetOneUser(id);
                    _userConnection.RemoveUser(id);
                    await Clients
                                .User(adminId)
                                .SendAsync("ReceiveListUserStatus", _userConnection.GetAllUser()); 
                }
            }

            await base.OnDisconnectedAsync(ex);
        }
        public async Task SendMessageAsync(string senderId, string receiveId, string message, string conversationId, string? originMessageId)
        {
            var messageDto = new MessageDto()
            {
                MessageId = Guid.NewGuid(),
                ConversationId = Guid.Parse(conversationId),
                MessageContent = message,
                OriginalMessageId = Guid.TryParse(originMessageId, out Guid id) ? id : null,
                SenderId = Guid.Parse(senderId)
            };

            var newMessage = await _messageService
                                            .PostMessageAsync(messageDto, CancellationToken.None);
            await Clients
                        .Users(senderId, receiveId)
                        .SendAsync("ReceiveMessage", newMessage);
        }
        public async Task GetMessageAsync(Guid conversationId)
        {
            var listMessage = await _messageService.GetListMessageAsync(conversationId, CancellationToken.None);
            await Clients
                        .Caller
                        .SendAsync("ReceiveMessages", listMessage);
        }
        public async Task GetConversationAsync(Guid conversationId)
        {
            var conversation = await _conversationService.GetConversationByIdAsync(conversationId, CancellationToken.None);
            await Clients
                        .Caller
                        .SendAsync("ReceiveConversation", conversation);
        }
        public async Task GetListParticipantAsync(string adminId)
        {
            var participentList = await _conversationService.GetListParticipationAsync(JsonConvert.SerializeObject(new
            {
                adminId
            }), CancellationToken.None);
            await Clients
                        .User(adminId)
                        .SendAsync("ReceiveUpdatedReceiveParticipants", participentList);
        }
        public async Task SendMessagePreparingAsync(string userId, bool isPreparing)
        {
            await Clients
                        .User(userId)
                        .SendAsync("ReceivePreparing", isPreparing);
        }
        private async Task SendListParticipant(string adminId)
        {
            var participentList = await _conversationService.GetListParticipationAsync(JsonConvert.SerializeObject(new
            {
                adminId
            }), CancellationToken.None);
            await Clients
                        .Caller
                        .SendAsync("ReceiveParticipants", participentList);
        }
    }
}
