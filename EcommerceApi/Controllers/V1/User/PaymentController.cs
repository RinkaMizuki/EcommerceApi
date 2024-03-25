using Asp.Versioning;
using EcommerceApi.Dtos.User;
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
        [Route("post")]
        public async Task<IActionResult> RedirectPayment(PaymentDto paymentDto,CancellationToken cancellationToken)
        {
            var newPayment = await _paymentService.PostPaymentOrderAsync(paymentDto, Request, cancellationToken);
            return Ok(newPayment);
        }
        [HttpGet]
        public async Task<IActionResult> ReturnPayment(CancellationToken cancellationToken)
        {

            var newPayment = await _paymentService.GetPaymentReturnAsync(Request, cancellationToken);
            return Ok(newPayment);
        }
    }
}
