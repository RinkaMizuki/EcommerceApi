using Asp.Versioning;
using EcommerceApi.Dtos.Admin;
using EcommerceApi.Models.IdentityData;
using EcommerceApi.Services.FeedbackService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApi.Controllers.V1.Admin
{
    //[Authorize(IdentityData.AdminPolicyName)]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/Admin/")]
    [Authorize]
    [ApiController]
    public class RateController : ControllerBase
    {
        private readonly IRateService _rateService;

        public RateController(IRateService rateService)
        {
            _rateService = rateService;
        }

        [HttpGet]
        [Route("rates/{rateId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRatingById(int rateId,
            CancellationToken userCancellationToken)
        {
            var ratingById =
                await _rateService.GetRateByIdAsync(rateId, userCancellationToken);
            return Ok(ratingById);
        }

        [HttpGet]
        [Route("rates")]
        [AllowAnonymous]
        public async Task<IActionResult> GetListRating(string sort, string range, string filter,
            CancellationToken userCancellationToken)
        {
            var listRating =
                await _rateService.GetListRateAsync(sort, range, filter, Response, userCancellationToken);
            return Ok(listRating);
        }

        [HttpPost]
        [Route("rates/post")]
        public async Task<IActionResult> CreateRating([FromBody]RateDto rateDto, CancellationToken userCancellationToken)
        {
            var rate = await _rateService.PostRateAsync(rateDto, userCancellationToken);
            if (rate == null) return BadRequest();
            return new JsonResult(rate);
        }

        [Authorize(Policy = "SsoAdmin")]
        [HttpPut]
        [Route("rates/update/{rateId:int}")]
        public async Task<IActionResult> UpdateRate(RateDto rateDto, int rateId,
            CancellationToken userCancellationToken)
        {
            var updateRate = await _rateService.UpdateRateAsync(rateDto, rateId, userCancellationToken);
            if (updateRate == null) return BadRequest();
            return Ok(updateRate);
        }

        [Authorize(Policy = "SsoAdmin")]
        [HttpDelete]
        [Route("rates/delete/{rateId:int}")]
        public async Task<IActionResult> DeleteRate(int rateId, CancellationToken userCancellationToken)
        {
            var result = await _rateService.DeleteRateAsync(rateId, userCancellationToken);
            if (!result) return BadRequest();
            return NoContent();
        }
    }
}