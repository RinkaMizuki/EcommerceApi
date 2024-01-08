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
        private readonly ICloudflareClientService _cloudflareClient;
        public ProductService(EcommerceDbContext context, ICloudflareClientService cloudflareClient) {
            _context = context;
            _cloudflareClient = cloudflareClient;
        }
        public async Task<bool> DeleteProductAsync(Guid productId, CancellationToken userCancellationToken)
        {
            var deleteProduct = await _context.Products
                                                .Where(p => p.ProductId == productId)
                                                .Include(p => p.ProductImages)
                                                .FirstOrDefaultAsync(userCancellationToken);
            if(deleteProduct is null) return false;            
            
            _context.Products.Remove(deleteProduct);
            await _context.SaveChangesAsync(userCancellationToken);
            var flag = false;
            foreach (var image in deleteProduct.ProductImages)
            {
                if(!flag)
                {
                    await _cloudflareClient.DeleteObjectAsync($"productImage_{deleteProduct.ProductId}_{deleteProduct.Image}", userCancellationToken);
                    flag = true;
                }
                await _cloudflareClient.DeleteObjectAsync($"productImage_{deleteProduct.ProductId}_{image.Image}",userCancellationToken);
            }
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

        public async Task<List<Product>> GetProductByCategoryAsync(int categoryId, CancellationToken userCancellationToken)
        {
            var productByCate = await _context
                                            .Products
                                            .Where(p => p.CategoryId == categoryId)
                                            .Include(p => p.ProductColors)
                                            .Include(p => p.ProductImages)
                                            .AsNoTracking()
                                            .ToListAsync(userCancellationToken);
            return productByCate;
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

        public async Task<Product> PostProductAsync(ProductDto productDto, string userName, HttpRequest request,CancellationToken userCancellationToken)
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
                newProduct.Url = $"{request.Scheme}://{request.Host}/api/v1/Admin/product/preview?productImage=productImage_{newProduct.ProductId}_{productDto.Files[0]?.FileName}";
            }

            List<ProductImage> listProductImage = new List<ProductImage>();

            bool flag = false;

            foreach (var file in productDto.Files)
            {
                await _cloudflareClient.UploadImageAsync(new UploadDto()
                {
                    Id = newProduct.ProductId,
                    File = file,
                }, prefix: "productImage", userCancellationToken);
                if (!flag)
                {
                    flag = true;
                    continue;
                };
                listProductImage.Add(new ProductImage()
                {
                    Image = file.FileName,
                    Url = $"{request.Scheme}://{request.Host}/api/v1/Admin/product/preview?productImage=productImage_{newProduct.ProductId}_{file.FileName}",
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

            await _context
                        .Products
                        .AddAsync(newProduct, userCancellationToken);
            await _context
                        .ProductImages
                        .AddRangeAsync(listProductImage, userCancellationToken);
            await _context
                        .ProductColors
                        .AddRangeAsync(listColor, userCancellationToken);
            await _context
                        .SaveChangesAsync(userCancellationToken);

            return newProduct;
        }

        public async Task<Product> UpdateProductAsync(ProductDto productDto, Guid productId, string userName, HttpRequest request, CancellationToken userCancellationToken)
        {
            var updateProduct = await _context
                                            .Products
                                            .Where(p => p.ProductId == productId)
                                            .Include(p => p.ProductImages)
                                            .Include(p => p.ProductColors)
                                            .FirstOrDefaultAsync(userCancellationToken);

            if (updateProduct == null) { return null ; }

            var listOldImage = updateProduct
                                        .ProductImages
                                        .Select(i => i.Image)
                                        .ToList();

            var listNewImage = productDto
                                        .Files
                                        .Select(f => f.FileName)
                                        .ToList();
            var listAddImage = productDto
                                        .Files
                                        .Where(f => !listOldImage.Contains(f.FileName) 
                                         || f.FileName == updateProduct.Image 
                                         && productId == updateProduct.ProductId)
                                        .Select(f => f)
                                        .ToList();
            var listDeleteImage = updateProduct
                                        .ProductImages
                                        .Where(i => !listNewImage.Contains(i.Image)
                                         && productId == updateProduct.ProductId)
                                        .Select(i => i)
                                        .ToList();

            // [1] [2,3,4] => [2,3,6,1] => thêm 6 và 1 , xóa 4 => [2] [3,6,1]

            // [2] [3,6,1] => [3,4,1,5] => thêm 4 và 5 , xóa 6 và 2 => [3] [4,1,5]

            // [3] [1,4,5] => [3,6,2,5] => thêm 3 và 6 và 2 ,xóa 1 và 4 => [3] [6,2,5]

            // [3] [6,2,5] => [1,2,3,4] => thêm 1 và 3 và 4 , xóa 5 và 6 => [1] [2,3,4]

            // [1] [2,3,4] => [5,2,4,1] => thêm 5 và 1 , xóa 3 => [5] [2,4,1]

            // [1] [2,3] => [1,2,3,4] => thêm 1 và 4, xóa => [1] [2,3,4]

            if (listDeleteImage.Count > 0)
            {
                foreach (var image in listDeleteImage)
                {
                    await _cloudflareClient.DeleteObjectAsync($"productImage_{updateProduct.ProductId}_{image.Image}", userCancellationToken);
                    _context.ProductImages.Remove(image);
                }
            }

            if (listAddImage.Count > 0)
            {
                foreach (var imageFile in listAddImage)
                {
                    if(imageFile.FileName != updateProduct.Image)
                    {
                        await _cloudflareClient.UploadImageAsync(new UploadDto()
                        {
                            File = imageFile,
                            Id = updateProduct.ProductId,
                        }, prefix: "productImage", userCancellationToken);
                        await _context.ProductImages.AddAsync(new ProductImage()
                        {
                            ProductImageId = Guid.NewGuid(),
                            Product = updateProduct,
                            ProductId = updateProduct.ProductId,
                            Image = imageFile.FileName,
                            Url = $"{request.Scheme}://{request.Host}/api/v1/Admin/product/preview?productImage=productImage_{updateProduct.ProductId}_{imageFile.FileName}",
                            CreatedAt = DateTime.Now,
                            ModifiedAt = DateTime.Now,
                        }, userCancellationToken);
                    }
                    if(imageFile.FileName == updateProduct.Image && imageFile.FileName != listNewImage[0])
                    {
                        await _context.ProductImages.AddAsync(new ProductImage()
                        {
                            ProductImageId = Guid.NewGuid(),
                            Product = updateProduct,
                            ProductId = updateProduct.ProductId,
                            Image = imageFile.FileName,
                            Url = $"{request.Scheme}://{request.Host}/api/v1/Admin/product/preview?productImage=productImage_{updateProduct.ProductId}_{imageFile.FileName}",
                            CreatedAt = DateTime.Now,
                            ModifiedAt = DateTime.Now,
                        }, userCancellationToken);
                    }
                }
            }

            if (!listNewImage.Contains(updateProduct.Image))
            {
                await _cloudflareClient.DeleteObjectAsync($"productImage_{updateProduct.ProductId}_{updateProduct.Image}", userCancellationToken);
                updateProduct.Image = listNewImage[0];
                updateProduct.Url = $"{request.Scheme}://{request.Host}/api/v1/Admin/product/preview?productImage=productImage_{updateProduct.ProductId}_{listNewImage[0]}";
                _context.ProductImages.Remove(updateProduct.ProductImages.Where(pi => pi.Image == updateProduct.Image).FirstOrDefault());
            }

            if (listNewImage.Contains(updateProduct.Image) && listNewImage[0] != updateProduct.Image) 
            {
                updateProduct.Image = listNewImage[0];
                updateProduct.Url = $"{request.Scheme}://{request.Host}/api/v1/Admin/product/preview?productImage=productImage_{updateProduct.ProductId}_{listNewImage[0]}";
                _context.ProductImages.Remove(updateProduct.ProductImages.Where(pi => pi.Image == updateProduct.Image).FirstOrDefault());
            }
            updateProduct.Title = productDto.Title;
            updateProduct.Description = productDto.Description;
            updateProduct.Quantity = productDto.Quantity;
            updateProduct.Price = productDto.Price;
            updateProduct.Status = productDto.Status;
            updateProduct.Discount = productDto.Discount;
            updateProduct.Hot = productDto.Hot;
            updateProduct.FlashSale = productDto.FlashSale;
            updateProduct.Upcoming = productDto.Upcoming;
            updateProduct.Return = productDto.Return;
            updateProduct.CategoryId = productDto.CategoryId;

            var listNewColor = Helpers.ParseString<string>(productDto.ColorCode);
            var listOldColor = updateProduct
                                           .ProductColors
                                           .Select(pc => pc.ColorCode)
                                           .ToList();
            var listDeleteColor = updateProduct
                                           .ProductColors
                                           .Where(pc => !listNewColor.Contains(pc.ColorCode) && productId == updateProduct.ProductId)
                                           .Select(pc => pc)
                                           .ToList();
            var listAddColor = listNewColor
                                           .Where(i => !listOldColor.Contains(i) && productId == updateProduct.ProductId)
                                           .ToList();

            _context
                   .ProductColors
                   .RemoveRange(listDeleteColor); // được đánh dấu là deteted

            foreach(var color in listAddColor)
            {
                await _context.ProductColors.AddAsync(new ProductColor() { 
                    ColorId = Guid.NewGuid(),
                    ColorCode = color,
                    Product = updateProduct,
                    ProductId = updateProduct.ProductId,
                }, userCancellationToken);
            }

            await _context.SaveChangesAsync(userCancellationToken);

            return updateProduct;
        }
    }
}
