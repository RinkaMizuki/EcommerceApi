using Asp.Versioning;
using EcommerceApi.Dtos.Admin;
using EcommerceApi.Models.IdentityData;
using EcommerceApi.Services.DestinateService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApi.Controllers.V1.Admin
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/Admin")]
    [ApiController]
    [Authorize(Policy = IdentityData.AdminPolicyName)]
    public class DestinateController : ControllerBase
    {
        private readonly IDestinateService _destinateService;
        public DestinateController(IDestinateService destinateService)
        {
            _destinateService = destinateService;
        }
        [HttpPost]
        [Route("destinate/post")]
        public async Task<IActionResult> PostDestinate(DestinationDto destinationDto, CancellationToken cancellationToken)
        {
            var newDes = await _destinateService.PostDestinationAsync(destinationDto, cancellationToken);
            return Ok(newDes);
        } 
    }
}
