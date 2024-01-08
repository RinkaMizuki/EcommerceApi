using EcommerceApi.Dtos.Admin;
using EcommerceApi.Models.Product;

namespace EcommerceApi.Services.ProductService
{
    public interface IProductService
    {
        public Task<List<Product>> GetListProductAsync();
        public Task<Product> GetProductByIdAsync(Guid productId);
        public Task<Boolean> DeleteProductAsync(Guid productId, CancellationToken userCancellationToken);
        public Task<Product> UpdateProductAsync(ProductDto productDto, Guid productId, string userName,HttpRequest request ,CancellationToken userCancellationToken);
        public Task<Product> PostProductAsync(ProductDto productDto, string userName, HttpRequest request, CancellationToken userCancellationToken);
        public Task<List<Product>> GetProductByCategoryAsync(int categoryId, CancellationToken userCancellationToken);
    }
}
