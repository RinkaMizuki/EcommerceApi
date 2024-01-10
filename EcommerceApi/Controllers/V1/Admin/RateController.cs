using Asp.Versioning;
using EcommerceApi.Dtos.Admin;
using EcommerceApi.Models.IdentityData;
using EcommerceApi.Services.FeedbackService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApi.Controllers.V1.Admin
{
    [Authorize(IdentityData.AdminPolicyName)]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/Admin/")]
    [ApiController]
    public class RateController : ControllerBase
    {
        private readonly IRateService _feedbackService;
        public RateController(IRateService feedbackService)
        {
            _feedbackService = feedbackService;
        }
        [HttpGet]
        [Route("rates")]
        public async Task<IActionResult> GetListFeedback(CancellationToken userCancellationToken)
        {

            var listFeedback = await _feedbackService.GetListFeedbackAsync(userCancellationToken);
            return Ok(listFeedback);

        }
        [HttpPost]
        [Route("rates/post")]
        public async Task<IActionResult> CreateRate(RateDto rateDto,CancellationToken userCancellationToken)
        {
            var rate = await _feedbackService.PostFeedbackAsync(rateDto, userCancellationToken);
            if (rate == null) return BadRequest();
            return new JsonResult(rate);

        }
        [HttpPut]
        [Route("rates/update/{rateId:int}")]
        public async Task<IActionResult> UpdateRate(RateDto rateDto, int rateId, CancellationToken userCancellationToken)
        {
            var updateRate = await _feedbackService.UpdateFeedbackAsync(rateDto, rateId, userCancellationToken);
            if (updateRate == null) return BadRequest();
            return Ok(updateRate);
        }
        [HttpDelete]
        [Route("rates/delete/{rateId:int}")]
        public async Task<IActionResult> DeleteRate(int rateId, CancellationToken userCancellationToken)
        {
            var result = await _feedbackService.DeleteFeedbackAsync(rateId, userCancellationToken);
            if (!result) return BadRequest();
            return NoContent();
        }
    }
}
