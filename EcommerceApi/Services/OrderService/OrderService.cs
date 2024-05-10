using EcommerceApi.Constant;
using EcommerceApi.Dtos.Admin;
using EcommerceApi.ExtensionExceptions;
using EcommerceApi.Models;
using EcommerceApi.Models.Order;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
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

        public async Task<List<Order>> GetListOrderAsync(string filter, string range, string sort, HttpResponse response, CancellationToken userCancellationToken)
        {
            try
            {
                var rangeValues = Helpers.ParseString<int>(range);

                if (rangeValues.Count == 0)
                {
                    rangeValues.AddRange(new List<int> { 0, 11 });
                }

                var sortValues = Helpers.ParseString<string>(sort);

                if (sortValues.Count == 0)
                {
                    sortValues.AddRange(new List<string> { "", "" });
                }

                var filterValues = Helpers.ParseString<string>(filter);

                var sortString = string.Join(", ", sortValues.Where((s, i) => i % 2 == 0)
                                           .Zip(sortValues.Where((s, i) => i % 2 != 0), (a, b) => $"{(a == "id" ? "orderId" : a)} {b}")).Trim();

            
                var perPage = rangeValues[1] - rangeValues[0] + 1;
                var currentPage = Convert.ToInt32(Math.Ceiling((double)rangeValues[0] / perPage)) + 1;

                var listOrderQuery = _context
                                        .Orders
                                        .Include(od => od.OrderDetails)
                                        .ThenInclude(odd => odd.Product);

                var listOrder = await listOrderQuery
                                                    .AsNoTracking()
                                                    .ToListAsync(userCancellationToken)
                                                    ?? new List<Order>();

                if (!string.IsNullOrEmpty(sortString))
                {
                    listOrder = listOrder.AsQueryable().OrderBy(sortString).ToList();
                }

                var listOrderPaging = Helpers.CreatePaging(listOrder, rangeValues, currentPage, perPage, "orders", response);

                return listOrderPaging;
            }
            catch (Exception ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<Order> GetOrderByIdAsync(Guid orderId, CancellationToken userCancellationToken)
        {
            try
            {
                var order = await _context
                                          .Orders
                                          .Where(u => u.OrderId.Equals(orderId))
                                          .Include(od => od.OrderDetails)
                                          .ThenInclude(odd => odd.Product)
                                          .AsNoTracking()
                                          .FirstOrDefaultAsync(userCancellationToken)
                                          ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Order not found.");
                return order;
            }
            catch(Exception ex)
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
                                                 .FirstOrDefaultAsync(userCancellationToken)
                                                 ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Order not found.");
               
                updateOrder.Status = orderDto.Status;

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
