using Asp.Versioning;
using EcommerceApi.Dtos.Admin;
using EcommerceApi.Services.ContactService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EcommerceApi.Controllers.V1.Admin
{
    //[Authorize(Policy = IdentityData.AdminPolicyName)]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/Admin/")]
    [Authorize]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly IContactService _contactService;

        public ContactController(IContactService contactService)
        {
            _contactService = contactService;
        }
        [HttpGet]
        [Authorize(Policy = "SsoAdmin")]
        [Route("contacts")]
        public async Task<IActionResult> GetListContact(string? sort, string? range, string? filter, CancellationToken userCancellationToken)
        {
            var listContact = await _contactService.GetListContactASync(sort, range, filter, Response, userCancellationToken);
            return Ok(listContact);
        }
        [HttpGet]
        [Authorize(Policy = "SsoAdmin")]
        [Route("contacts/{contactId:int}")]
        public async Task<IActionResult> GetContactById(int contactId, CancellationToken userCancellationToken)
        {
            var contact = await _contactService.GetContactByIdAsync(contactId, userCancellationToken);
            return Ok(contact);
        }
        [HttpDelete]
        [Authorize(Policy = "SsoAdmin")]
        [Route("contacts/delete/{contactId:int}")]
        public async Task<IActionResult> DeleteContact(int contactId, CancellationToken userCancellationToken)
        {
            await _contactService.DeleteContactAsync(contactId, userCancellationToken);
            return Ok(new { 
                message = "Delete successfully",
                statusCode = HttpStatusCode.NoContent,
            });
        }
        [HttpPost]
        [Route("contacts/post")]
        public async Task<IActionResult> CreateContact([FromForm]ContactDto contactDto, CancellationToken userCancellationToken)
        {
            var newContact = await _contactService.PostContactAsync(contactDto, userCancellationToken);
            return StatusCode(201, newContact);
        }
        [HttpPut]
        [Route("contacts/update/{contactId:int}")]
        [Authorize(Policy = "SsoAdmin")]
        public async Task<IActionResult> SupportContact([FromBody]ContactDto contactDto, CancellationToken userCancellationToken)
        {
            await _contactService.PostSupportContactAsync(contactDto, userCancellationToken);
            return StatusCode(200, new
            {
                message = "Send email successfully.",
                statusCode = 200,
                id =  Guid.NewGuid(),
            });
        }
    }
}
