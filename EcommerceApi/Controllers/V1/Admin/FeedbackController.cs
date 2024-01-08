using Asp.Versioning;
using EcommerceApi.Models.IdentityData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApi.Controllers.V1.Admin
{
    [Authorize(IdentityData.AdminPolicyName)]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersiom}/Admin/")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {

    }
}
