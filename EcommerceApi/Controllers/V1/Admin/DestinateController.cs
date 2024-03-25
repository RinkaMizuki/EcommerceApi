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
        [Route("destinates/post")]
        public async Task<IActionResult> PostDestinate(DestinationDto destinationDto, CancellationToken cancellationToken)
        {
            var newDes = await _destinateService.PostDestinationAsync(destinationDto, cancellationToken);
            return Ok(newDes);
        }
        [HttpPut]
        [Route("destinates/update/{destinationId}")]
        public async Task<IActionResult> PostDestinate(DestinationDto destinationDto, Guid destinationId, CancellationToken cancellationToken)
        {
            var updateDes = await _destinateService.UpdateDestinationAsync(destinationDto, destinationId, cancellationToken);
            return Ok(updateDes);
        }
        [HttpDelete]
        [Route("destinates/delete/{destinationId}")]
        public async Task<IActionResult> DeleteDestinate(Guid destinationId, CancellationToken cancellationToken)
        {
            await _destinateService.DeleteDestinationAsync(destinationId, cancellationToken);
            return StatusCode(204, new { 
                message = "Delete destinate successfully",
                statusCode = 204,
            });
        }
        [AllowAnonymous]
        [HttpGet]
        [Route("destinates")]
        public async Task<IActionResult> GetListDestinate(CancellationToken cancellationToken)
        {
            var listDes = await _destinateService.GetListDestinationAsync(cancellationToken);
            return Ok(listDes);
        }
    }
}
