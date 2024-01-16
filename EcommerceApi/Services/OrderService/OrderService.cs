using EcommerceApi.Dtos.Admin;
using EcommerceApi.ExtensionExceptions;
using EcommerceApi.Models;
using EcommerceApi.Models.Order;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace EcommerceApi.Services.OrderService
{
    public class OrderService : IOrderService
    {
        private readonly EcommerceDbContext _context;
        public OrderService(EcommerceDbContext context)
        {
            _context = context;
        }
        public async Task<bool> DeleteOrderAsync(Guid orderId, CancellationToken userCancellationToken)
        {
            try
            {
                var deleteOrder = _context
                                  .Orders
                                  .Where(o => o.OrderId == orderId)
                                  .FirstOrDefaultAsync(userCancellationToken)
                                  ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Order not found.");
                _context.Remove(deleteOrder);
                await _context.SaveChangesAsync(userCancellationToken);
                return true;
            }
            catch(DbUpdateException ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }                             

        public async Task<List<Order>> GetListOrderAsync(CancellationToken userCancellationToken)
        {
            try
            {
                var listOrder = await _context
                                              .Orders
                                              .Include(o => o.OrderDetails)
                                              .ThenInclude(od => od.Product)
                                              .AsNoTracking()
                                              .ToListAsync(userCancellationToken)
                                              ?? new List<Order>();
                return listOrder;
            }
            catch (DbUpdateException ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<Order> UpdateOrderAsync(OrderDto orderDto,Guid orderId, CancellationToken userCancellationToken)
        {
            try
            {
                var updateOrder = await  _context
                                                 .Orders
                                                 .Where(o => o.OrderId == orderId)
                                                 .Include(o => o.OrderDetails)
                                                 .FirstOrDefaultAsync(userCancellationToken)
                                                 ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Order not found.");
               
                updateOrder.Status = orderDto.Status;
                updateOrder.Returned = orderDto.Returned;

                await _context.SaveChangesAsync(userCancellationToken);
                return updateOrder;
            }
            catch( DbUpdateException ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
