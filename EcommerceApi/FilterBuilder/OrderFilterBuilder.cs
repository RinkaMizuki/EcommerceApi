using EcommerceApi.Constant;
using EcommerceApi.Models.Order;

namespace EcommerceApi.FilterBuilder
{
    public class OrderFilterBuilder
    {
        private readonly List<Func<Order, bool>> _filterOptions = new();

        //options
        public OrderFilterBuilder AddStatusFilter(string status)
        {
            if (!string.IsNullOrEmpty(status))
            {
                if(status == OrderFilterType.Returned)
                {
                    _filterOptions.Add(od => od.Returned);
                }
                else if(status == OrderFilterType.Ordered)
                {
                    _filterOptions.Add(od => od.Status.ToLower().Equals(status) && !od.Returned);
                }
                else if(status == OrderFilterType.Cancelled)
                {
                    _filterOptions.Add(od => od.Status.ToLower().Equals(status) && !od.Returned);
                }
                else if(status == OrderFilterType.Delivered)
                {
                    _filterOptions.Add(od => od.Status.ToLower().Equals(status));
                }
                else
                {
                    _filterOptions.Add(od => od.Status.ToLower().Equals(status));//shipped
                }
            }
            return this;
        }
        public OrderFilterBuilder AddIdFilter(string orderId)
        {
            if (!string.IsNullOrEmpty(orderId))
            {
                _filterOptions.Add(od => od.OrderId.Equals(new Guid(orderId)));
            }
            return this;
        }
        public OrderFilterBuilder AddUserFilter(string userId) {
            if(!string.IsNullOrEmpty(userId))
            {
                _filterOptions.Add(od => od.User.UserId.Equals(new Guid(userId)));
            }
            return this;
        }

        public OrderFilterBuilder AddBeforeDateFilter(string beforeDate)
        {
            if (!string.IsNullOrEmpty(beforeDate))
            {
                _filterOptions.Add(od => DateTime.Compare(Convert.ToDateTime(od.OrderDate.ToShortDateString()),
                    Convert.ToDateTime(beforeDate)) <= 0);
            }

            return this;
        }

        public OrderFilterBuilder AddSinceDateFilter(string sinceDate)
        {
            if (!string.IsNullOrEmpty(sinceDate))
            {
                _filterOptions.Add(od =>
                    DateTime.Compare(Convert.ToDateTime(od.OrderDate.ToShortDateString()),
                        Convert.ToDateTime(sinceDate)) >= 0);
            }
            return this;
        }

        //public OrderFilterBuilder AddReturnedFilter(string returned)
        //{
        //    if (!string.IsNullOrEmpty(returned))
        //    {
        //        _filterOptions.Add(od => od.Returned.Equals(Convert.ToBoolean(returned)));
        //    }
        //    return this;
        //}

        //public OrderFilterBuilder AddCancelledFilter(string cancelled)
        //{
        //    if (!string.IsNullOrEmpty(cancelled))
        //    {
        //        _filterOptions.Add(od => od.Status.ToLower().Equals(cancelled.ToLower()));
        //    }
        //    return this;
        //}

        //public OrderFilterBuilder AddOrderedFilter(string ordered)
        //{
        //    if(!string.IsNullOrEmpty(ordered))
        //    {
        //        _filterOptions.Add(od => od.Status.ToLower() != "cancelled" && !od.Returned);
        //    }
        //    return this;
        //}
        public OrderFilterBuilder AddAmountFilter(string amount)
        {
            if (!string.IsNullOrEmpty(amount))
            {
                _filterOptions.Add(od => od.TotalPrice >= Convert.ToDecimal(amount));
            }
            return this;
        }

        public Func<Order, bool> Build() => od => _filterOptions.All(filter => filter(od));
    }
}
