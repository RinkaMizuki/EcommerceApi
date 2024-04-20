using Asp.Versioning;
using EcommerceApi.Dtos.Admin;
using EcommerceApi.Models.IdentityData;
using EcommerceApi.Services.FeedbackRateService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApi.Controllers.V1.Admin
{
    [ApiVersion("1.0")]
    //[Authorize(Policy = IdentityData.AdminPolicyName)]
    [Authorize(Policy = "SsoAdmin")]
    [Route("api/v{version:apiVersion}/Admin/")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;
        public FeedbackController(IFeedbackService feedbackService) {
            _feedbackService = feedbackService;
        }
        [HttpGet]
        [Route("feedbacks")]
        [AllowAnonymous]
        public async Task<IActionResult> GetListFeedback(CancellationToken userCancellationToken)
        {
            var listFeedbacks = await _feedbackService.GetListFeedbackAsync(userCancellationToken);
            return Ok(listFeedbacks);
        }
        [HttpPost]
        [Route("feedbacks/post/{rateId:int}")]
        public async Task<IActionResult> CreateFeedback(FeedbackDto feedbackDto, int rateId, CancellationToken userCancellationToken)
        {
            var userName = Helpers.GetUserNameLogin(HttpContext);
            var feedbackRes = await _feedbackService.PostFeedbackAsync(feedbackDto, rateId, userName, userCancellationToken);
            return Ok(feedbackRes);
        }
        [HttpDelete]
        [Route("feedbacks/delete/{feedbackId:int}")]
        public async Task<IActionResult> DeleteFeedback(int feedbackId, CancellationToken userCancellationToken)
        {
            await _feedbackService.DeleteFeedbackAsync(feedbackId, userCancellationToken);
            return NoContent();
        }
        [HttpPut]
        [Route("feedbacks/update/{feedbackId:int}")]
        public async Task<IActionResult> UpdateFeedback(FeedbackDto feedbackDto, int feedbackId, CancellationToken userCancellationToken) {
            var userName = Helpers.GetUserNameLogin(HttpContext);
            var feedbackRes = await _feedbackService.UpdateFeedbackAsync(feedbackDto, userName, feedbackId, userCancellationToken);
            if (feedbackRes is null) return BadRequest();
            return Ok(feedbackRes);
        }
    }
}
