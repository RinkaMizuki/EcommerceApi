using Azure.Core;
using EcommerceApi.Dtos.Admin;
using EcommerceApi.Dtos.Upload;
using EcommerceApi.Models;
using EcommerceApi.Models.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace EcommerceApi.Services.ProductService
{
    public class ProductService : IProductService
    {
        private readonly EcommerceDbContext _context;
        private readonly ICloudflareClient _cloudflareClient;
        public ProductService(EcommerceDbContext context, ICloudflareClient cloudflareClient) {
            _context = context;
            _cloudflareClient = cloudflareClient;
        }
        public async Task<bool> DeleteProductAsync(Guid productId)
        {
            var deleteProduct = await _context.Products.FindAsync(productId);
            if(deleteProduct is null) return false;
            _context.Products.Remove(deleteProduct);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Product>> GetListProductAsync()
        {
            return await _context.Products
                                        .Include(p => p.ProductImages)
                                        .Include(p => p.ProductColors)
                                        .AsNoTracking()
                                        .ToListAsync();
        }

        public async Task<Product> GetProductByIdAsync(Guid productId)
        {
            var productById = await _context.Products
                                                     .Where(p => p.ProductId == productId)
                                                     .Include(p => p.ProductImages)
                                                     .Include(p => p.ProductColors)
                                                     .AsNoTracking()
                                                     .FirstOrDefaultAsync();
            return productById;
        }

        public async Task<Product> PostProductAsync(ProductDto productDto, string userName, HttpRequest request)
        {
            var newProduct = new Product() {
                ProductId = Guid.NewGuid(),
                Title = productDto.Title,
                Description = productDto.Description,
                Price = productDto.Price,
                Discount = productDto.Discount,
                Status = productDto.Status,
                Hot = productDto.Hot,
                FlashSale = productDto.FlashSale,
                Upcoming = productDto.Upcoming,
                Quantity = productDto.Quantity,
                Return = productDto.Return,
                CreatedAt = DateTime.Now,
                CreatedBy = userName,
                ProductCategory = await _context.ProductCategories.FindAsync(productDto.CategoryId),
                CategoryId = productDto.CategoryId,
            };

            if (productDto.Files.Length > 0)
            {
                newProduct.Image = productDto.Files[0]?.FileName;
                newProduct.Url = $"{request.Scheme}://{request.Host}/api/v1/Admin/users/preview?productImage=productImage_{newProduct.ProductId}_{productDto.Files[0]?.FileName}";
            }
            List<ProductImage> listProductImage = new List<ProductImage>();
            bool flag = false;
            foreach (var file in productDto.Files)
            {
                await _cloudflareClient.UploadImageAsync(new UploadDto()
                {
                    Id = newProduct.ProductId,
                    File = file,
                }, "productImage");
                if (!flag)
                {
                    flag = true;
                    continue;
                };
                listProductImage.Add(new ProductImage()
                {
                    Image = file.FileName,
                    Url = $"{request.Scheme}://{request.Host}/api/v1/Admin/users/preview?productImage=productImage_{newProduct.ProductId}_{file.FileName}",
                    ProductId = newProduct.ProductId,
                    CreatedAt = DateTime.Now,
                    Product = newProduct,
                    ProductImageId = Guid.NewGuid(),
                });
            }
            var listColor = new List<ProductColor>();
            foreach(var color in Helpers.ParseString<string>(productDto.ColorCode))
            {
                listColor.Add(new ProductColor() {
                    ProductId = newProduct.ProductId,
                    ColorCode = color,
                    ColorId = Guid.NewGuid(),
                    Product = newProduct,
                });
            }
            await _context.Products.AddAsync(newProduct);
            await _context.ProductImages.AddRangeAsync(listProductImage);
            await _context.ProductColors.AddRangeAsync(listColor);
            await _context.SaveChangesAsync();
            return newProduct;
        }

        public async Task<Product> UpdateProductAsync(ProductDto productDto, Guid productId, string userName)
        {
            throw new NotImplementedException();
        }
    }
}
