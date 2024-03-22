using Asp.Versioning;
using EcommerceApi.Config;
using EcommerceApi.Services.PaymentService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

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
        [HttpGet]
        [Route("redirect")]
        public IActionResult RedirectToPayment(CancellationToken cancellationToken)
        {
            var redirectUrl = _paymentService.GetPaymentOrder(Request ,cancellationToken);
            return Ok(redirectUrl);
        }
    }
}
