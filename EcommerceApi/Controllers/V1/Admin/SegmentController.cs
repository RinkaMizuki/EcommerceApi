using Asp.Versioning;
using EcommerceApi.Dtos.Admin;
using EcommerceApi.Models.IdentityData;
using EcommerceApi.Services.SegmentService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApi.Controllers.V1.Admin
{
    [ApiVersion("1.0")]
    [Authorize(Policy = IdentityData.AdminPolicyName)]
    [Route("api/v{version:apiVersion}/Admin/")]
    [ApiController]
    public class SegmentController : ControllerBase
    {
        private readonly ISegmentService _segmentService;

        public SegmentController(ISegmentService segmentService)
        {
            _segmentService = segmentService;
        }
        [HttpGet]
        [Route("segments")]
        public async Task<IActionResult> GetListSegment(CancellationToken userCancellationToken)
        {
            var listSegment = await _segmentService.GetListSegmentAsync(userCancellationToken);
            return Ok(listSegment);
        }
        [HttpPut]
        [Route("segments/update/{segmentId:int}")]
        public async Task<IActionResult> UpdateSegment(SegmentDto segmentDto, int segmentId, CancellationToken userCancellationToken)
        {
            var updateSegment = await _segmentService.UpdateSegmentAsync(segmentDto, segmentId, userCancellationToken);
            return Ok(updateSegment);
        }
        [HttpDelete]
        [Route("segments/delete/{segmentId:int}")]
        public async Task<IActionResult> DeleteSegment(int segmentId, CancellationToken userCancellationToken)
        {
            await _segmentService.DeleteSegmentAsync(segmentId, userCancellationToken);
            return Ok(new
            {
                message = "Delete successfully",
                statusCode = 204,
            });
        }
        [HttpPost]
        [Route("segments/post")]
        public async Task<IActionResult> CreateSegment(SegmentDto segmentDto, CancellationToken userCancellationToken)
        {
            var newSegment = await _segmentService.PostSegmentAsync(segmentDto, userCancellationToken);
            return Ok(newSegment);
        }
    }
}
