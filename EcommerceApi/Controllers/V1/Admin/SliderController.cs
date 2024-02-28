using Asp.Versioning;
using EcommerceApi.Dtos.Admin;
using EcommerceApi.Models.IdentityData;
using EcommerceApi.Services.SliderService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EcommerceApi.Controllers.V1.Admin
{
    [Route("api/v{version:apiVersion}/Admin")]
    [ApiVersion("1.0")]
    [ApiController]
    [Authorize(Policy = IdentityData.AdminPolicyName)]
    public class SliderController : ControllerBase
    {
        private readonly ISliderService _sliderService;
        public SliderController(ISliderService sliderService) {
            _sliderService = sliderService;
        }
        [HttpGet]
        [Route("sliders")]
        public async Task<IActionResult> GetListSlider(CancellationToken cancellationToken)
        {
            var listSlider = await _sliderService.GetListCouponAsync(cancellationToken);
            return Ok(listSlider);
        }

        [HttpPost]
        [Route("sliders/post")]
        public async Task<IActionResult> PostSlider([FromForm]SliderDto sliderDto,CancellationToken cancellationToken)
        {
            var slider = await _sliderService.PostSliderAsync(sliderDto, Request, cancellationToken);
            return Ok(slider);
        }

        [HttpPut]
        [Route("sliders/update/{sliderId:Guid}")]
        public async Task<IActionResult> UpdateSlider([FromForm] SliderDto sliderDto, Guid sliderId, CancellationToken cancellationToken)
        {
            var currentUser = Helpers.GetUserNameLogin(HttpContext);
            var slider = await _sliderService.UpdateSliderAsync(sliderDto, sliderId, Request, currentUser, cancellationToken);
            return Ok(slider);
        }
        [HttpDelete]
        [Route("sliders/delete/{sliderId:Guid}")]
        public async Task<IActionResult> DeleteSlider(Guid sliderId, CancellationToken cancellationToken)
        {
            await _sliderService.DeleteSliderAsync(sliderId, cancellationToken);
            return StatusCode((int)HttpStatusCode.NoContent, new
            {
                message = "Delete slider successfully",
                statusCode = 204,
            });
        }
        [AllowAnonymous]
        [HttpGet]
        [Route("slider/preview")]
        public async Task<IActionResult> GetProductImage([FromQuery]string sliderImage, CancellationToken userCancellationToken)
        {
            var userAvatar = await _sliderService.GetImageAsync(sliderImage, userCancellationToken);
            return File(userAvatar.FileStream, userAvatar.ContentType);
        }
    }
}
