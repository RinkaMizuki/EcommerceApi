using Asp.Versioning;
using EcommerceApi.Dtos.Admin;
using EcommerceApi.Models.IdentityData;
using EcommerceApi.Services.ContactService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EcommerceApi.Controllers.V1.Admin
{
    [ApiVersion("1.0")]
    //[Authorize(Policy = IdentityData.AdminPolicyName)]
    [Authorize(Policy = "SsoAdmin")]
    [Route("api/v{version:apiVersion}/Admin/")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly IContactService _contactService;

        public ContactController(IContactService contactService)
        {
            _contactService = contactService;
        }
        [HttpGet]
        [Route("contacts")]
        public async Task<IActionResult> GetListContact(CancellationToken userCancellationToken)
        {
            var listContact = await _contactService.GetListContactASync(userCancellationToken);
            return Ok(listContact);
        }
        [HttpGet]
        [Route("contacts/{contactId:int}")]
        public async Task<IActionResult> GetContactById(int contactId, CancellationToken userCancellationToken)
        {
            var contact = await _contactService.GetContactByIdAsync(contactId, userCancellationToken);
            return Ok(contact);
        }
        [HttpDelete]
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
    }
}
