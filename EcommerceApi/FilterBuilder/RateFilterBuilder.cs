using EcommerceApi.Constant;
using EcommerceApi.Models.Rate;

namespace EcommerceApi.FilterBuilder;

public class RateFilterBuilder
{
    private readonly List<Func<Rate, bool>> _filterOptions = new();

    //options
    public RateFilterBuilder AddSearchFilter(string searchValue)
    {
        if (!string.IsNullOrEmpty(searchValue))
        {
            _filterOptions.Add(rate => rate.Content.ToLower().Contains(searchValue.ToLower()));
        }

        return this;
    }

    public RateFilterBuilder AddUserFilter(string filterValue)
    {
        if (!string.IsNullOrEmpty(filterValue))
        {
            _filterOptions.Add(rate => rate.UserId.Equals(Convert.ToInt32(filterValue)));
        }

        return this;
    }

    public RateFilterBuilder AddProductFilter(string filterValue)
    {
        if (!string.IsNullOrEmpty(filterValue))
        {
            _filterOptions.Add(rate => rate.ProductId.Equals(new Guid(filterValue)));
        }

        return this;
    }

    public RateFilterBuilder AddStatusFilter(string filterValue)
    {
        if (!string.IsNullOrEmpty(filterValue))
        {
            _filterOptions.Add(rate => rate.Status.Equals(filterValue));
        }

        return this;
    }

    public RateFilterBuilder AddBeforeDateFilter(string filterValue)
    {
        if (!string.IsNullOrEmpty(filterValue))
        {
            _filterOptions.Add(rate => DateTime.Compare(Convert.ToDateTime(rate.CreatedAt.ToShortDateString()),
                Convert.ToDateTime(filterValue)) <= 0);
        }

        return this;
    }

    public RateFilterBuilder AddSinceDateFilter(string filterValue)
    {
        if (!string.IsNullOrEmpty(filterValue))
        {
            _filterOptions.Add(rate =>
                DateTime.Compare(Convert.ToDateTime(rate.CreatedAt.ToShortDateString()),
                    Convert.ToDateTime(filterValue)) >= 0);
        }

        return this;
    }
    
    //build filter with many options
    public Func<Rate, bool> Build() => rate => _filterOptions.All(filter => filter(rate));
}