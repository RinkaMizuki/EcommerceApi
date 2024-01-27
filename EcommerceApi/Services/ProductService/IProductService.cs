using EcommerceApi.Dtos.Admin;
using EcommerceApi.Models.Product;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApi.Services.ProductService
{
    public interface IProductService
    {
        public Task<List<Product>> GetListProductAsync(string sort, string range, string filter, HttpResponse response,
            CancellationToken userCancellationToken);

        public Task<Product> GetProductByIdAsync(Guid productId, CancellationToken userCancellationToken);
        public Task<Boolean> DeleteProductAsync(Guid productId, CancellationToken userCancellationToken);

        public Task<Product> UpdateProductAsync(ProductDto productDto, Guid productId, string userName,
            HttpRequest request, CancellationToken userCancellationToken);

        public Task<Product> PostProductAsync(ProductDto productDto, string userName, HttpRequest request,
            CancellationToken userCancellationToken);

        public Task<List<Product>> GetProductByCategoryAsync(int categoryId, CancellationToken userCancellationToken);
        public Task<FileStreamResult> GetImageAsync(string imageUrl, CancellationToken userCancellationToken);
    }
}