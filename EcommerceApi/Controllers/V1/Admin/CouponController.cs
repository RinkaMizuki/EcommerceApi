using Asp.Versioning;
using EcommerceApi.Dtos.Admin;
using EcommerceApi.Models.Coupon;
using EcommerceApi.Models.IdentityData;
using EcommerceApi.Services.CouponService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

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
        [AllowAnonymous]
        public async Task<IActionResult> GetListCoupon(string? sort, string? range, string? filter, CancellationToken userCancellationToken)
        {
           var listCoupon = await _couponService.GetListCouponAsync(sort, range, filter, Response, userCancellationToken);
           return Ok(listCoupon);
        }
        [HttpGet]
        [Route("coupons/{couponId:Guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCouponById(Guid couponId, CancellationToken userCancellationToken)
        {
            var couponById = await _couponService.GetCouponByIdAsync(couponId, userCancellationToken);
            return StatusCode(Convert.ToInt32(HttpStatusCode.Created), couponById);
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
        public async Task<IActionResult> UpdateCoupon(Coupon couponDto, Guid couponId, CancellationToken userCancellationToken)
        {
            var updateCoupon = await _couponService.UpdateCouponAsync(couponDto, couponId, userCancellationToken);
            return Ok(updateCoupon);
        }
    }
}
