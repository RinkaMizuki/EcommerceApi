using EcommerceApi.Constant;
using EcommerceApi.ExtensionExceptions;
using EcommerceApi.FilterBuilder;
using EcommerceApi.Models;
using EcommerceApi.Models.Order;
using EcommerceApi.Models.Payment;
using EcommerceApi.Responses;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Net;

namespace EcommerceApi.Services.InvoiceService
{
    public class InvoiceService : IInvoiceService
    {
        private readonly EcommerceDbContext _context;
        private readonly InvoiceFilterBuilder _paymentFilter;
        public InvoiceService(EcommerceDbContext context, InvoiceFilterBuilder paymentFilter) {
            _context = context;
            _paymentFilter = paymentFilter;
        }
        public async Task<List<InvoiceResponse>> GetListInvoiceAsync(string filter, string range, string sort, HttpResponse response, CancellationToken cancellationToken)
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
                                           .Zip(sortValues.Where((s, i) => i % 2 != 0), (a, b) => $"{a} {b}")).Trim();

                if (!filterValues.Contains(PaymentFilterType.Customer))
                {
                    filterValues.Add(PaymentFilterType.Customer);
                    filterValues.Add("");
                }
                if (!filterValues.Contains(PaymentFilterType.Order))
                {
                    filterValues.Add(PaymentFilterType.Order);
                    filterValues.Add("");
                }
                if (!filterValues.Contains(PaymentFilterType.PassedBefore))
                {
                    filterValues.Add(PaymentFilterType.PassedBefore);
                    filterValues.Add("");
                }
                if (!filterValues.Contains(PaymentFilterType.PassedSince))
                {
                    filterValues.Add(PaymentFilterType.PassedSince);
                    filterValues.Add("");
                }

                //get filter value
                var customerId = filterValues[filterValues.IndexOf(PaymentFilterType.Customer) + 1].ToString().ToLower();
                var orderId = filterValues[filterValues.IndexOf(PaymentFilterType.Order) + 1].ToString();
                var passedBefore = filterValues[filterValues.IndexOf(PaymentFilterType.PassedBefore) + 1];
                var passedSince = filterValues[filterValues.IndexOf(PaymentFilterType.PassedSince) + 1];

                var filters = _paymentFilter
                                    .AddCustomerFilter(customerId)
                                    .AddOrderFilter(orderId)
                                    .AddBeforeDateFilter(passedBefore)
                                    .AddSinceDateFilter(passedSince)
                                    .Build();

                var listPaymentQuery = _context
                                        .Payments
                                        .Include(pm => pm.PaymentDestination)
                                        .Include(pm => pm.Order)
                                        .ThenInclude(od => od.User);
                                       


                var listPayment = await listPaymentQuery
                                                    .AsNoTracking()
                                                    .ToListAsync(cancellationToken)
                                                    ?? new List<Payment>();
                var listInvoice = listPayment
                                        .Where(filters)
                                        .Select(pm => new InvoiceResponse {
                                            Id = pm.PaymentId,
                                            Order = pm.Order,
                                            User = pm.Order.User,
                                            PaymentDestination = pm.PaymentDestination,
                                            PaymentContent = pm.PaymentContent,
                                            PaymentMessage = pm.PaymentLastMessage,
                                            PaymentCurrency = pm.PaymentCurrency,
                                            PaymentStatus = pm.PaymentStatus,
                                            PaidAmout = pm.PaidAmout,
                                            CreatedAt = pm.CreatedAt
                                        })
                                        .ToList();
                //get paging
                var perPage = rangeValues[1] - rangeValues[0] + 1;
                var currentPage = Convert.ToInt32(Math.Ceiling((double)rangeValues[0] / perPage)) + 1;

                if (!string.IsNullOrEmpty(sortString))
                {
                    listInvoice = listInvoice.AsQueryable().OrderBy(sortString).ToList();
                }

                var listInvoicePaging = Helpers.CreatePaging(listInvoice, rangeValues, currentPage, perPage, "orders", response);

                return listInvoicePaging;
            }
            catch(Exception ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
