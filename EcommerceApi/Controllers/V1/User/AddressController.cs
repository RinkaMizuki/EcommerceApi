using Asp.Versioning;
using EcommerceApi.Dtos.User;
using EcommerceApi.Services.AddressService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EcommerceApi.Controllers.V1.User
{
    [ApiVersion("1.0")]
    [Authorize]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;
        public AddressController(IAddressService addressService) {
            _addressService = addressService;
        }

        [Route("{userId:guid}")]
        [HttpGet]
        public async Task<IActionResult> GetListAddress(Guid userId, CancellationToken cancellationToken) {
            var listAddress = await _addressService.GetListUserAddressAsync(userId, cancellationToken);
            return StatusCode((int)HttpStatusCode.OK, listAddress);
        }

        [Route("post/{userId:guid}")]
        [HttpPost]
        public async Task<IActionResult> PostAddress([FromBody]UserAddressDto userAddressDto, Guid userId, CancellationToken cancellationToken)
        {
            var newAddress = await _addressService.PostUserAddressAsync(userAddressDto, userId, cancellationToken);
            return StatusCode((int)HttpStatusCode.OK, newAddress);
        }

        [Route("delete/{addressId}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteAddress(Guid addressId, CancellationToken cancellationToken)
        {
            await _addressService.DeleteUserAddressAsync(addressId, cancellationToken);
            return StatusCode((int)HttpStatusCode.NoContent, new
            {
                message = "Delete successfully",
                statusCode = 204,
            });
        }

        [Route("update/{addressId}")]
        [HttpPut]
        public async Task<IActionResult> UpdateAddress([FromBody]UserAddressDto userAddressDto, Guid addressId, CancellationToken cancellationToken)
        {
            var updateAddress = await _addressService.UpdateUserAddressAsync(userAddressDto, addressId, cancellationToken);
            return StatusCode((int)HttpStatusCode.OK, updateAddress);
        }

    }
}
