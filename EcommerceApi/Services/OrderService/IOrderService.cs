using EcommerceApi.Dtos.Admin;
using EcommerceApi.Models.Order;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApi.Services.OrderService
{
    public interface IOrderService
    {
        public Task<List<Order>> GetListOrderAsync(string filter, string range, string sort, HttpResponse response, CancellationToken userCancellationToken);
        public Task<bool> DeleteOrderAsync(Guid orderId, CancellationToken userCancellationToken);
        public Task<Order> UpdateOrderAsync(OrderDto orderDto, Guid orderId, CancellationToken userCancellationToken);
        public Task<Order> GetOrderByIdAsync(Guid orderId, CancellationToken userCancellationToken);
    }
}
