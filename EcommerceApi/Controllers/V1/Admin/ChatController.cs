using Asp.Versioning;
using EcommerceApi.Services.ChatService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApi.Controllers.V1.Admin
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/Admin/")]
    [Authorize]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IConversationService _conversationService;
        public ChatController(IConversationService conversationService)
        {
            _conversationService = conversationService;
        }
        [HttpGet("conversations")]
        public async Task<IActionResult> GetListConversation(string? filter, CancellationToken cancellationToken)
        {
            var listConversation = await _conversationService.GetListParticipationAsync(filter, cancellationToken);
            return StatusCode(200, listConversation);
        }
    }
}
