using EcommerceApi.Constant;
using EcommerceApi.Dtos.Admin;
using EcommerceApi.ExtensionExceptions;
using EcommerceApi.FilterBuilder;
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
        private readonly OrderFilterBuilder _orderFilter;
        public OrderService(EcommerceDbContext context, OrderFilterBuilder orderFilter)
        {
            _context = context;
            _orderFilter = orderFilter;
        }
        public async Task<bool> DeleteOrderAsync(Guid orderId, CancellationToken userCancellationToken)
        {
            try
            {
                var deleteOrder = await _context
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
                var filterValues = Helpers.ParseString<string>(filter);

                if (rangeValues.Count == 0)
                {
                    rangeValues.AddRange(new List<int> { 0, 11 });
                }

                var sortValues = Helpers.ParseString<string>(sort);

                if (sortValues.Count == 0)
                {
                    sortValues.AddRange(new List<string> { "", "" });
                }

             
                var sortString = string.Join(", ", sortValues.Where((s, i) => i % 2 == 0)
                                           .Zip(sortValues.Where((s, i) => i % 2 != 0), (a, b) => $"{(a == "id" ? "orderId" : a)} {b}")).Trim();


                //get paging
                var perPage = rangeValues[1] - rangeValues[0] + 1;
                var currentPage = Convert.ToInt32(Math.Ceiling((double)rangeValues[0] / perPage)) + 1;


                if (!filterValues.Contains(OrderFilterType.Status))
                {
                    filterValues.Add(OrderFilterType.Status);
                    filterValues.Add("");
                }
                if (!filterValues.Contains(OrderFilterType.UserId))
                {
                    filterValues.Add(OrderFilterType.UserId);
                    filterValues.Add("");
                }
                if (!filterValues.Contains(OrderFilterType.OrderId))
                {
                    filterValues.Add(OrderFilterType.OrderId);
                    filterValues.Add("");
                }

                if (!filterValues.Contains(OrderFilterType.Before))
                {
                    filterValues.Add(OrderFilterType.Before);
                    filterValues.Add("");
                }

                if (!filterValues.Contains(OrderFilterType.Since))
                {
                    filterValues.Add(OrderFilterType.Since);
                    filterValues.Add("");
                }
                if (!filterValues.Contains(OrderFilterType.Returned))
                {
                    filterValues.Add(OrderFilterType.Returned);
                    filterValues.Add("");
                }
                if (!filterValues.Contains(OrderFilterType.MinAmount))
                {
                    filterValues.Add(OrderFilterType.MinAmount);
                    filterValues.Add("");
                }

                //get filter value
                var status = filterValues[filterValues.IndexOf(OrderFilterType.Status) + 1].ToString().ToLower();
                var userId = filterValues[filterValues.IndexOf(OrderFilterType.UserId) + 1].ToString();
                var orderId = filterValues[filterValues.IndexOf(OrderFilterType.OrderId) + 1].ToString();
                var orderedBefore = filterValues[filterValues.IndexOf(OrderFilterType.Before) + 1];
                var orderedSince = filterValues[filterValues.IndexOf(OrderFilterType.Since) + 1];
                var returned = filterValues[filterValues.IndexOf(OrderFilterType.Returned) + 1];
                var minAmount = filterValues[filterValues.IndexOf(OrderFilterType.MinAmount) + 1];

                var filters = _orderFilter
                                     .AddStatusFilter(status)
                                     .AddUserFilter(userId)
                                     .AddIdFilter(orderId)
                                     .AddBeforeDateFilter(orderedBefore)
                                     .AddSinceDateFilter(orderedSince)
                                     .AddReturnedFilter(returned)
                                     .AddAmountFilter(minAmount)
                                     .Build();

                var listOrderQuery = _context
                                        .Orders
                                        .Include(od => od.User)
                                        .Include(od => od.Coupon)
                                        .Include(od => od.OrderDetails)
                                        .ThenInclude(odd => odd.Product);
             

                var listOrder = await listOrderQuery
                                                    .AsNoTracking()
                                                    .ToListAsync(userCancellationToken)
                                                    ?? new List<Order>();
                listOrder = listOrder.Where(filters).ToList();
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
                                          .Include(od => od.Coupon)
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
                updateOrder.Returned = orderDto.Returned;

                await _context.SaveChangesAsync(userCancellationToken);
                return updateOrder;
            }
            catch(Exception ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
