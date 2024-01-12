using Asp.Versioning;
using EcommerceApi.Dtos.Admin;
using EcommerceApi.Models.IdentityData;
using EcommerceApi.Services.CouponService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApi.Controllers.V1.Admin
{
    [Authorize(IdentityData.AdminPolicyName)]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/Admin/")]
    [ApiController]
    public class CouponController : ControllerBase
    {
        private readonly ICouponService _couponService;

        public CouponController(ICouponService couponService)
        {
            _couponService = couponService;
        }
        [HttpGet]
        [Route("coupons")]
        public async Task<IActionResult> GetListCoupon(CancellationToken userCancellationToken)
        {
           var listCoupon = await _couponService.GetListCouponAsync(userCancellationToken);
           return Ok(listCoupon);
        }
        [HttpPost]
        [Route("coupons/post")]
        public async Task<IActionResult> CreateCoupon(CouponDto couponDto, CancellationToken userCancellationToken)
        {
            var newCoupon = await _couponService.PostCouponAsync(couponDto, userCancellationToken);
            return Ok(newCoupon);
        }
        [HttpDelete]
        [Route("coupons/delete/{couponId:Guid}")]
        public async Task<IActionResult> DeleteCoupon(Guid couponId, CancellationToken userCancellationToken)
        {
            await _couponService.DeleteCouponAsync(couponId, userCancellationToken);
            return Ok(new { 
                message = "Delete coupon successfully",
                statusCode = 204,
            });
        }
        [HttpPut]
        [Route("coupons/update/{couponId:Guid}")]
        public async Task<IActionResult> UpdateCoupon([FromQuery]bool isActive,Guid couponId, CancellationToken userCancellationToken)
        {
            var updateCoupon = await _couponService.UpdateCouponAsync(isActive, couponId, userCancellationToken);
            return Ok(updateCoupon);
        }
    }
}
