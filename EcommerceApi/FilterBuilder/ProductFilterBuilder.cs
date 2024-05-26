using EcommerceApi.Models.Product;

namespace EcommerceApi.FilterBuilder
{
    public class ProductFilterBuilder
    {
        private readonly List<Func<Product, bool>> _filterOptions = new();
        //options
        public ProductFilterBuilder AddQuantityFilter(int minStock, int maxStock)
        {
            if (minStock >= 0 && maxStock >= 0)
            {
                _filterOptions.Add(prod => prod.ProductStock.StockQuantity >= minStock && prod.ProductStock.StockQuantity <= maxStock);
            }
            else if(minStock > 0 && maxStock < 0)
            {
                _filterOptions.Add(prod => prod.ProductStock.StockQuantity >= minStock);
            }
            else if(maxStock > 0 && minStock < 0)
            {
                _filterOptions.Add(prod => prod.ProductStock.StockQuantity <= maxStock);
            }
            return this;
        }

        public ProductFilterBuilder AddCategoryFilter(string category)
        {
            if (!string.IsNullOrEmpty(category))
            {
                _filterOptions.Add(prod => prod.CategoryId.ToString().Equals(category)
                || prod.ProductCategory.Title.ToLower().Equals(category)
                || prod.ProductCategory.ParentCategoryId.ToString().Equals(category)
                || (prod.ProductCategory.ParentProductCategory != null && prod.ProductCategory.ParentProductCategory.Title.ToLower().Equals(category))
                );
            }
            return this;
        }
        public ProductFilterBuilder AddSearchFilter(string search)
        {
            if (!string.IsNullOrEmpty(search))
            {
                _filterOptions.Add(prod => prod.Title.ToLower().Contains(search.ToLower()));
            }
            return this;
        }
        public ProductFilterBuilder AddSaleFilter(string saleType)
        {
            if (!string.IsNullOrEmpty(saleType))
            {
                _filterOptions.Add(prod => (saleType == "hot" && prod.Hot) 
                                            ||(saleType == "flashsale" && prod.FlashSale) 
                                            ||(saleType == "upcoming" && prod.Upcoming)
                );
            }
            return this;
        }
        public ProductFilterBuilder AddPriceSaleFilter(int minPrice, int maxPrice)
        {
            _filterOptions.Add(prod => Helpers.CalcPriceSale(prod.Price, prod.Discount) >= minPrice && Helpers.CalcPriceSale(prod.Price, prod.Discount) <= maxPrice);
            return this;
        }
        public Func<Product, bool> Build() => prod => _filterOptions.All(filter => filter(prod));
    }
}
