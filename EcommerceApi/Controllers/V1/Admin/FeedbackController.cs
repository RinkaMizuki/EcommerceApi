using Asp.Versioning;
using EcommerceApi.Dtos.Admin;
using EcommerceApi.Models.IdentityData;
using EcommerceApi.Services.FeedbackRateService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApi.Controllers.V1.Admin
{
    [ApiVersion("1.0")]
    [Authorize(Policy = IdentityData.AdminPolicyName)]
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
        public async Task<IActionResult> GetListFeedback(CancellationToken userCancellationToken)
        {
           try
           {
               var listFeedbacks = await _feedbackService.GetListFeedbackAsync(userCancellationToken);
               return Ok(listFeedbacks);
           }
           catch (Exception ex)
           {
                return BadRequest(ex.Message);
           }
        }
        [HttpPost]
        [Route("feedbacks/post/{rateId:int}")]
        public async Task<IActionResult> CreateFeedback(FeedbackDto feedbackDto, int rateId, CancellationToken userCancellationToken)
        {
            try
            {
                var userName = Helpers.GetUserNameLogin(HttpContext);
                var feedbackRes = await _feedbackService.PostFeedbackAsync(feedbackDto, rateId, userName, userCancellationToken);

                if (feedbackRes is null) return BadRequest();
                
                return Ok(feedbackRes);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete]
        [Route("feedbacks/delete/{feedbackId:int}")]
        public async Task<IActionResult> DeleteFeedback(int feedbackId, CancellationToken userCancellationToken)
        {
            try
            {
                var feedbackRes = await _feedbackService.DeleteFeedbackAsync(feedbackId, userCancellationToken);
                if(!feedbackRes) return BadRequest();
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut]
        [Route("feedbacks/update/{feedbackId:int}")]
        public async Task<IActionResult> UpdateFeedback(FeedbackDto feedbackDto, int feedbackId, CancellationToken userCancellationToken) {
            try
            {
                var userName = Helpers.GetUserNameLogin(HttpContext);
                var feedbackRes = await _feedbackService.UpdateFeedbackAsync(feedbackDto, userName, feedbackId, userCancellationToken);
                if(feedbackRes is null) return BadRequest();
                return Ok(feedbackRes);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
