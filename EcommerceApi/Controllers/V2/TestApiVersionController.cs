using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApi.Controllers.EcommerceV2
{
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class TestApiVersionController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return new JsonResult("Test");
        }
    }
}
