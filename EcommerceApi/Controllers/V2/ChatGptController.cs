using Asp.Versioning;
using EcommerceApi.Services.OpenaiService;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApi.Controllers.V2
{
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class ChatGptController : ControllerBase
    {
        private readonly IOpenaiService _openaiService;

        public ChatGptController(IOpenaiService openaiService)
        {
            _openaiService = openaiService;
        }

        [HttpGet]
        public async Task<IActionResult> GetChatGPTResponse([FromQuery] string prompt)
        {
            var response = await _openaiService.GetChatGPTResponse(prompt);
            return Ok(response);
        }
    }
}
