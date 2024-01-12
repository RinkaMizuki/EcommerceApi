using Asp.Versioning;
using EcommerceApi.Models.IdentityData;
using EcommerceApi.Models.Message;
using EcommerceApi.Services.MailService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApi.Controllers.V1.Admin
{
    [ApiVersion("1.0")]
    [Authorize(Policy = IdentityData.AdminPolicyName)]
    [Route("api/v{version:apiVersion}/Admin/")]
    [ApiController]
    public class MailController : ControllerBase
    {
        private readonly IMailService _mailService;
        public MailController(IMailService mailService) { 
            _mailService = mailService;
        }
        [HttpGet("send-email")]
        public async Task<IActionResult> SendEmail(CancellationToken userCancellation)
        {
            var message = new Message(new List<string>{ "dh52107825@student.stu.edu.vn"}, "Test email", "This is the content from our email.");
            await _mailService.SendEmailAsync(message, userCancellation);

            return Ok(new
            {
                message = "Send email successfully",
                statusCode = 200,
            });
        }
    }
}
