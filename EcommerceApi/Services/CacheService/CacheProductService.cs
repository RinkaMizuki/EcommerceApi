using Azure.Core;
using EcommerceApi.Constant;
using EcommerceApi.Dtos.Admin;
using EcommerceApi.Models.Product;
using EcommerceApi.Responses;
using EcommerceApi.Services.ProductService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Caching.Memory;

namespace EcommerceApi.Services.CacheService
{
    public class CacheProductService : IProductService
    {
        private readonly IProductService _productService;
        private readonly IMemoryCache _cache;
        public CacheProductService(IProductService productService,IMemoryCache cache)
        {
            _productService = productService;
            _cache = cache;
        }

        public async Task<bool> DeleteProductAsync(Guid productId, CancellationToken userCancellationToken)
        {
            _cache.Remove($"{CacheKey.Product}_{productId}");
            _cache.Remove(CacheKey.Product);
            return await _productService.DeleteProductAsync(productId, userCancellationToken);
        }

        public async Task<FileStreamResult> GetImageAsync(string imageUrl, CancellationToken userCancellationToken)
        {
            return await _productService.GetImageAsync(imageUrl, userCancellationToken);
        }

        public async Task<List<Product>> GetListProductAsync(string sort, string range, string filter, HttpResponse response, CancellationToken userCancellationToken)
        {
            //var productCache = await _cache.GetOrCreateAsync(CacheKey.Product, async entry => {
            //    entry.SlidingExpiration = TimeSpan.FromSeconds(60);
            //    entry.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5);
            //});
            var productCache = await _productService.GetListProductAsync(sort,  range,  filter, response, userCancellationToken);
            
            //List<int> rangeValues = Helpers.ParseString<int>(range);
            //if (rangeValues.Count == 0)
            //{
            //    rangeValues.AddRange(new List<int> { 0, 4 });
            //};
            
            //response.Headers.Append("Access-Control-Expose-Headers", "Content-Range");
            //response.Headers.Append("Content-Range", $"products {rangeValues[0]}-{rangeValues[1]}/{productCache?.Count}");
            return productCache ?? new List<Product>();
        }

        public async Task<List<Product>> GetProductByCategoryAsync(int categoryId, CancellationToken userCancellationToken)
        {
            return await _productService.GetProductByCategoryAsync(categoryId, userCancellationToken);
        }

        public async Task<Product> GetProductByIdAsync(Guid productId, CancellationToken userCancellationToken)
        {
            //if cache exist product by id => return data in cache , otherwise go to service and query data in db then set cache and response
            var product = await _cache.GetOrCreateAsync($"{CacheKey.Product}_{productId}", async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromSeconds(60);
                entry.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5);
                return await _productService.GetProductByIdAsync(productId, userCancellationToken);
            });
            return product!;
        }

        public async Task<Product> PostProductAsync(ProductDto productDto, string userName, HttpRequest request, CancellationToken userCancellationToken)
        {
            _cache.Remove(CacheKey.Product);
            return await _productService.PostProductAsync(productDto, userName, request, userCancellationToken);
        }

        public async Task<Product> UpdateProductAsync(ProductDto productDto, Guid productId, string userName, HttpRequest request, CancellationToken userCancellationToken)
        {
            _cache.Remove($"{CacheKey.Product}_{productId}");
            _cache.Remove(CacheKey.Product);
            return await _productService.UpdateProductAsync(productDto, productId, userName, request, userCancellationToken);
        }
    }
}
