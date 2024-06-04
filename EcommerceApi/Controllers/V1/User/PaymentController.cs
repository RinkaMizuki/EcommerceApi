using Asp.Versioning;
using EcommerceApi.Dtos.User;
using EcommerceApi.Lib;
using EcommerceApi.Services.PaymentService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApi.Controllers.V1.User
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        public PaymentController(IPaymentService paymentService) {
            _paymentService = paymentService;
        }
        [HttpPost]
        [Route("payos/post")]
        public async Task<IActionResult> RedirectPaymentPayOs(PaymentDto paymentDto, CancellationToken cancellationToken)
        {
            var newCheckout = await _paymentService.PostPaymentPayOSOrderAsync(paymentDto, Request, cancellationToken);
            return Ok(newCheckout);
        }
        [HttpPost]
        [Route("payos/return")]
        public async Task<IActionResult> ReturnPayOsPayment(CancellationToken cancellationToken)
        {

            var newPayment = await _paymentService.PostPaymentPayOSReturnAsync(Request, cancellationToken);
            return Ok(newPayment);
        }
        [HttpPost]
        [Route("vnpay/post")]
        public async Task<IActionResult> RedirectPaymentVnPay(PaymentDto paymentDto,CancellationToken cancellationToken)
        {
            var newCheckout = await _paymentService.PostPaymentVnPayOrderAsync(paymentDto, Request, cancellationToken);
            return Ok(newCheckout);
        }
        [HttpPost]
        [Route("vnpay/return")]
        public async Task<IActionResult> ReturnVnPayPayment(CancellationToken cancellationToken)
        {

            var newPayment = await _paymentService.PostPaymentVnPayReturnAsync(Request, cancellationToken);
            return Ok(newPayment);
        }
        [HttpGet]
        [Route("vnpay/ipn")]
        [AllowAnonymous]
        public async Task<IActionResult> IpnVnPayPayment(CancellationToken cancellationToken)
        {
            var ipnResponse = await _paymentService.GetPaymentVnPayIpnAsync(Request, cancellationToken);
            return Ok(ipnResponse);
        }
    }
}
