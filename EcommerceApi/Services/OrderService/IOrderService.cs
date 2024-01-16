using EcommerceApi.Dtos.Admin;
using EcommerceApi.Models.Order;

namespace EcommerceApi.Services.OrderService
{
    public interface IOrderService
    {
        public Task<List<Order>> GetListOrderAsync(CancellationToken userCancellationToken);
        public Task<bool> DeleteOrderAsync(Guid orderId, CancellationToken userCancellationToken);
        public Task<Order> UpdateOrderAsync(OrderDto orderDto, Guid orderId, CancellationToken userCancellationToken);
    }
}
