using EcommerceApi.Models.Payment;

namespace EcommerceApi.FilterBuilder
{
    public class InvoiceFilterBuilder
    {
        private readonly List<Func<Payment, bool>> _filterOptions = new();
        //options
        public InvoiceFilterBuilder AddCustomerFilter(string customId) {
            if (!string.IsNullOrEmpty(customId)) {
                _filterOptions.Add(pm => pm.Order.UserId.Equals(new Guid(customId)));
            }
            return this;
        }

        public InvoiceFilterBuilder AddOrderFilter(string orderId)
        {
            if (!string.IsNullOrEmpty(orderId))
            {
                _filterOptions.Add(pm => pm.Order.OrderId.Equals(new Guid(orderId)));
            }
            return this;
        }
        public InvoiceFilterBuilder AddBeforeDateFilter(string beforeDate)
        {
            if (!string.IsNullOrEmpty(beforeDate))
            {
                _filterOptions.Add(pm => DateTime.Compare(Convert.ToDateTime(pm.CreatedAt.ToShortDateString()),
                    Convert.ToDateTime(beforeDate)) <= 0);
            }

            return this;
        }

        public InvoiceFilterBuilder AddSinceDateFilter(string sinceDate)
        {
            if (!string.IsNullOrEmpty(sinceDate))
            {
                _filterOptions.Add(pm =>
                    DateTime.Compare(Convert.ToDateTime(pm.CreatedAt.ToShortDateString()),
                        Convert.ToDateTime(sinceDate)) >= 0);
            }
            return this;
        }
        public Func<Payment, bool> Build() => pm => _filterOptions.All(filter => filter.Invoke(pm));
    }
}
