using Asp.Versioning;
using EcommerceApi.Dtos.Admin;
using EcommerceApi.Models.IdentityData;
using EcommerceApi.Services.OrderService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApi.Controllers.V1.Admin
{
    [ApiVersion("1.0")]
    //[Authorize(Policy = IdentityData.AdminPolicyName)]
    [Authorize(Policy = "SsoAdmin")]
    [Route("api/v{version:apiVersion}/Admin/")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        [HttpGet]
        [Route("orders")]
        public async Task<IActionResult> GetListOrder(CancellationToken cancellationToken)
        {
            var listOrder = await _orderService.GetListOrderAsync(cancellationToken);
            return StatusCode(200, listOrder);
        }
        [HttpDelete]
        [Route("oders/delele/{orderId:guid}")]
        public async Task<IActionResult> DeleteOrder(Guid orderId, CancellationToken cancellationToken)
        {
            await _orderService.DeleteOrderAsync(orderId, cancellationToken);
            return StatusCode(204, new
            {
                message = "Delete order successfully",
                StatusCode = 204
            });
        }
        [HttpPut]
        [Route("oders/update/{orderId:guid}")]
        public async Task<IActionResult> UpdateOrder(OrderDto orderDto, Guid orderId, CancellationToken cancellationToken)
        {
            var updateOrder = await _orderService.UpdateOrderAsync(orderDto, orderId, cancellationToken);
            return Ok(updateOrder);
        }
    }
}
