using Asp.Versioning;
using EcommerceApi.Dtos.User;
using EcommerceApi.Services.CouponService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EcommerceApi.Controllers.V1.User
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICouponService _couponService;
        public CartController(ICouponService couponService) {
            _couponService = couponService;
        }

        [HttpPost]
        [Route("coupon")]
        public async Task<IActionResult> ApplyCoupon([FromBody]CouponProductDto couponProductDto, CancellationToken cancellationToken)
        {
            var totalPrice = await _couponService.ApplyCouponProductAsync(couponProductDto, cancellationToken);
            return StatusCode((int)HttpStatusCode.OK, new
            {
                message = "Applied coupon successfully.",
                statusCode = 200,
                totalPrice,
            });
        }
    }
}
