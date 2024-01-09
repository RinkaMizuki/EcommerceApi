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
    public class FeedbackController : ControllerBase
    {
        private readonly IRateService _feedbackService;
        public FeedbackController(IRateService feedbackService)
        {
            _feedbackService = feedbackService;
        }
        [HttpGet]
        [Route("feedbacks")]
        public async Task<IActionResult> GetListFeedback(CancellationToken userCancellationToken)
        {
            try
            {
                var listFeedback = await _feedbackService.GetListFeedbackAsync(userCancellationToken);
                return Ok(listFeedback);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        [Route("feedbacks/post")]
        public async Task<IActionResult> CreateRate(RateDto rateDto,CancellationToken userCancellationToken)
        {
            try
            {
                var rate = await _feedbackService.PostFeedbackAsync(rateDto, userCancellationToken);
                if (rate == null) return BadRequest();
                return new JsonResult(rate);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut]
        [Route("feedbacks/update/{rateId:int}")]
        public async Task<IActionResult> UpdateRate(RateDto rateDto, int rateId, CancellationToken userCancellationToken)
        {
            try
            {
                var updateRate = await _feedbackService.UpdateFeedbackAsync(rateDto, rateId, userCancellationToken);
                if (updateRate == null) return BadRequest();
                return Ok(updateRate);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete]
        [Route("feedbacks/delete/{rateId:int}")]
        public async Task<IActionResult> DeleteRate(int rateId, CancellationToken userCancellationToken)
        {
            try
            {
                var result = await _feedbackService.DeleteFeedbackAsync(rateId, userCancellationToken);
                if(!result) return BadRequest();
                return NoContent();
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
