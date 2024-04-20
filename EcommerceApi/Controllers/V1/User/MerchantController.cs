using Asp.Versioning;
using EcommerceApi.Dtos.User;
using EcommerceApi.Models.IdentityData;
using EcommerceApi.Services.MerchantService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApi.Controllers.V1.User
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}")]
    [ApiController]
    //[Authorize(Policy = IdentityData.AdminPolicyName)]
    [Authorize(Policy = "SsoAdmin")]
    public class MerchantController : ControllerBase
    {
        private readonly IMerchantService _merchantService;
        public MerchantController(IMerchantService merchantService)
        {
            _merchantService = merchantService;
        }
        [HttpGet]
        [Route("merchants")]
        public async Task<IActionResult> GetListMerchant(CancellationToken cancellationToken)
        {
            var listMerchant = await _merchantService.GetListMerchant(cancellationToken);
            return StatusCode(200, listMerchant);
        }
        [HttpPost]
        [Route("merchants/post")]
        public async Task<IActionResult> PostMerchant([FromBody]MerchantDto merchantDto, CancellationToken cancellationToken)
        {
            var newMerchant = await _merchantService.PostMerchantAsync(merchantDto, cancellationToken);
            return StatusCode(200, newMerchant);
        }
        [HttpPut]
        [Route("merchants/update/{merchantId}")]
        public async Task<IActionResult> UpdateMerchant([FromBody]MerchantDto merchantDto, [FromRoute]Guid merchantId, CancellationToken cancellationToken)
        {
            var updateMerchant = await _merchantService.UpdateMerchantAsync(merchantDto, merchantId, cancellationToken);
            return StatusCode(200, updateMerchant);
        }
        [HttpDelete]
        [Route("merchants/delete/{merchantId}")]
        public async Task<IActionResult> DeleteMerchant([FromRoute]Guid merchantId, CancellationToken cancellationToken)
        {
            await _merchantService.DeleteMerchantAsync(merchantId, cancellationToken);
            return StatusCode(204, new
            {
                statusCode = 204,
                message = "Delete merchant successfully",
            });
        }
    }
}
