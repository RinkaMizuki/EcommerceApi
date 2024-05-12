using Asp.Versioning;
using EcommerceApi.Services.InvoiceService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApi.Controllers.V1.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize]
    [Route("api/v{version:apiVersion}/Admin/")]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;
        public InvoiceController(IInvoiceService invoiceService) {
            _invoiceService = invoiceService;
        }
        [HttpGet]
        [Route("invoices")]
        public async Task<IActionResult> GetListInvoice([FromQuery]string filter, [FromQuery]string range, [FromQuery]string sort, CancellationToken cancellationToken)
        {
            var listInvoice = await _invoiceService.GetListInvoiceAsync(filter,range,sort,Response, cancellationToken);
            return StatusCode(200, listInvoice);
        }
    }
}
