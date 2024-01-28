using EcommerceApi.Dtos.Admin;
using EcommerceApi.Dtos.Upload;
using EcommerceApi.ExtensionExceptions;
using EcommerceApi.Models;
using EcommerceApi.Models.Product;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Net;
using EcommerceApi.Constant;
using SortOrder = EcommerceApi.Constant.SortOrder;

namespace EcommerceApi.Services.ProductService
{
    public class ProductService : IProductService
    {
        private readonly EcommerceDbContext _context;
        private readonly ICloudflareClientService _cloudflareClient;

        public ProductService(EcommerceDbContext context, ICloudflareClientService cloudflareClient)
        {
            _context = context;
            _cloudflareClient = cloudflareClient;
        }

        public async Task<bool> DeleteProductAsync(Guid productId, CancellationToken userCancellationToken)
        {
            try
            {
                var deleteProduct = await _context.Products
                                        .Where(p => p.ProductId == productId)
                                        .Include(p => p.ProductImages)
                                        .FirstOrDefaultAsync(userCancellationToken)
                                    ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Product not found.");

                _context.Products.Remove(deleteProduct);

                await _context.SaveChangesAsync(userCancellationToken);

                var flag = false;

                foreach (var image in deleteProduct.ProductImages)
                {
                    if (!flag)
                    {
                        await _cloudflareClient.DeleteObjectAsync(
                            $"productImage_{deleteProduct.ProductId}_{deleteProduct.Image}", userCancellationToken);
                        flag = true;
                    }

                    await _cloudflareClient.DeleteObjectAsync($"productImage_{deleteProduct.ProductId}_{image.Image}",
                        userCancellationToken);
                }

                return true;
            }
            catch (SqlException ex)
            {
                throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
            }
        }

        public async Task<List<Product>> GetListProductAsync(string sort, string range, string filter,
            HttpResponse response, CancellationToken userCancellationToken)
        {
            try
            {
                var rangeValues = Helpers.ParseString<int>(range);

                if (rangeValues.Count == 0)
                {
                    rangeValues.AddRange(new List<int> { 0, 4 });
                }

                ;
                var sortValues = Helpers.ParseString<string>(sort);

                if (sortValues.Count == 0)
                {
                    sortValues.AddRange(new List<string> { "", "" });
                }

                var filterValues = Helpers.ParseString<string>(filter);
                if (!filterValues.Contains(ProductFilterType.Search))
                {
                    filterValues.Insert(0, ProductFilterType.Search);
                    filterValues.Insert(1, "");
                }
                else
                {
                    var search = filterValues.IndexOf(ProductFilterType.Search) + 1;
                    filterValues.Insert(0, filterValues[search]);
                    filterValues.Insert(0, ProductFilterType.Search);

                    if (filterValues[^1] ==
                        ProductFilterType.Search) // filterValues[filterValues.Count - 1] == filterValues[^1]
                    {
                        filterValues.RemoveAt(filterValues.LastIndexOf(ProductFilterType.Search) - 1);
                        filterValues.RemoveAt(filterValues.LastIndexOf(ProductFilterType.Search));
                    }
                    else
                    {
                        filterValues.RemoveAt(filterValues.LastIndexOf(ProductFilterType.Search) + 1);
                        filterValues.RemoveAt(filterValues.LastIndexOf(ProductFilterType.Search));
                    }
                }

                if (!filterValues.Contains(ProductFilterType.Category))
                {
                    filterValues.Add(ProductFilterType.Category);
                    filterValues.Add("");
                }

                if (!filterValues.Contains(ProductFilterType.Sale))
                {
                    filterValues.Add(ProductFilterType.Sale);
                    filterValues.Add("");
                }

                if (!filterValues.Contains(ProductFilterType.StockRange))
                {
                    filterValues.Add(ProductFilterType.StockRange);
                    filterValues.Add("");
                    filterValues.Add("");
                }
                else
                {
                    var indexActive = filterValues.IndexOf(ProductFilterType.StockRange);
                    filterValues.Add(ProductFilterType.StockRange);
                    filterValues.Add(filterValues[indexActive + 1]);
                    filterValues.Add(filterValues[indexActive + 2]);
                    filterValues.Remove(ProductFilterType.StockRange);
                    filterValues.Remove(filterValues[indexActive + 1]);
                    filterValues.Remove(filterValues[indexActive]);
                }

                var stockRange = filterValues
                    .Skip(filterValues.IndexOf(ProductFilterType.StockRange) + 1)
                    .Take(2)
                    .ToList();


                var perPage = rangeValues[1] - rangeValues[0] + 1;
                var currentPage = Convert.ToInt32(Math.Ceiling((double)rangeValues[0] / perPage)) + 1;
                var sortBy = sortValues[0].ToLower();
                var sortType = sortValues[1].ToLower();

                var listProductQuery = _context
                    .Products
                    .Include(p => p.ProductCategory)
                    .Include(p => p.ProductImages)
                    .Include(p => p.ProductColors)
                    .Include(p => p.ProductRates);

                //conditions filter
                var minStock = string.IsNullOrEmpty(stockRange[0]) ? -999 : Convert.ToInt32(stockRange[0]);
                var maxStock = string.IsNullOrEmpty(stockRange[1]) ? -999 : Convert.ToInt32(stockRange[1]);
                var category = filterValues[filterValues.IndexOf(ProductFilterType.Category) + 1];
                var searchValue = filterValues[filterValues.IndexOf(ProductFilterType.Search) + 1].ToLower();
                var sale = filterValues[filterValues.IndexOf(ProductFilterType.Sale) + 1].ToLower();

                var listProduct = await listProductQuery
                    .AsNoTracking()
                    .ToListAsync(userCancellationToken);

                var totalProduct = await listProductQuery.CountAsync(userCancellationToken);

                var filterRefIds = new List<Guid>();
                if (filterValues.Contains(UserFilterType.Id))
                {
                    var keyStartIndex = filterValues.IndexOf(ProductFilterType.Id);
                    var keyEndIndex = filterValues.IndexOf(ProductFilterType.Category);
                    var listId = filterValues
                        .Skip(keyStartIndex + 1)
                        .Take(keyEndIndex - keyStartIndex - 1)
                        .Select(id => new Guid(id))
                        .ToList();
                    filterRefIds.AddRange(listId);
                    listProduct = listProduct
                        .Where(p => filterRefIds.Contains(p.ProductId))
                        .ToList();
                }

                listProduct = listProduct
                    .Where(p => ((p.Quantity >= minStock
                                  && p.Quantity <= maxStock)
                                 || (p.Quantity >= minStock
                                     && maxStock < 0)
                                 || (minStock < 0
                                     && maxStock < 0))
                                && (string.IsNullOrEmpty(category)
                                    || p.CategoryId == Convert.ToInt32(category) ||
                                    p.ProductCategory.ParentCategoryId == Convert.ToInt32(category))
                                && (string.IsNullOrEmpty(searchValue) ||
                                    p.Title.ToLower().Contains(searchValue.ToLower()))
                                && (string.IsNullOrEmpty(sale) || ((sale == "hot" && p.Hot) ||
                                                                   (sale == "flashsale" && p.FlashSale) ||
                                                                   (sale == "upcoming" && p.Upcoming))
                                )
                    ).ToList();

                switch (sortType)
                {
                    case "asc":
                        listProduct = sortBy switch
                        {
                            SortOrder.SortById => listProduct.OrderBy(p => p.ProductId).ToList(),
                            SortOrder.SortByStock => listProduct.OrderBy(p => p.Quantity).ToList(),
                            _ => listProduct.OrderBy(p => p.Title).ToList()
                        };

                        break;
                    case "desc":
                        listProduct = sortBy switch
                        {
                            SortOrder.SortById => listProduct.OrderByDescending(p => p.ProductId).ToList(),
                            SortOrder.SortByStock => listProduct.OrderByDescending(p => p.Quantity).ToList(),
                            _ => listProduct.OrderByDescending(p => p.Title).ToList()
                        };

                        break;
                }

                listProduct = listProduct
                    .Skip((currentPage - 1) * perPage)
                    .Take(perPage).ToList();

                response.Headers.Append("Access-Control-Expose-Headers", "Content-Range");
                response.Headers.Append("Content-Range", $"products {rangeValues[0]}-{rangeValues[1]}/{totalProduct}");

                return listProduct;
            }
            catch (SqlException ex)
            {
                throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
            }
        }

        public async Task<List<Product>> GetProductByCategoryAsync(int categoryId,
            CancellationToken userCancellationToken)
        {
            try
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
            catch (SqlException ex)
            {
                throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
            }
        }

        public async Task<Product> GetProductByIdAsync(Guid productId, CancellationToken userCancellationToken)
        {
            try
            {
                var productById = await _context.Products
                                      .Where(p => p.ProductId == productId)
                                      .Include(p => p.ProductImages)
                                      .Include(p => p.ProductColors)
                                      .Include(p => p.ProductRates)
                                      .AsNoTracking()
                                      .FirstOrDefaultAsync(userCancellationToken)
                                  ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Product not found.");

                return productById;
            }
            catch (SqlException ex)
            {
                throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
            }
        }

        public async Task<Product> PostProductAsync(ProductDto productDto, string userName, HttpRequest request,
            CancellationToken userCancellationToken)
        {
            try
            {
                var category =
                    await _context.ProductCategories.FindAsync(new object?[] { productDto.CategoryId },
                        cancellationToken: userCancellationToken) ??
                    throw new HttpStatusException(HttpStatusCode.NotFound, "Category not found.");

                var newProduct = new Product()
                {
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
                    ProductCategory = category,
                    CategoryId = productDto.CategoryId,
                };

                if (productDto.Files.Length > 0)
                {
                    newProduct.Image = productDto.Files[0].FileName;
                    newProduct.Url =
                        $"{request.Scheme}://{request.Host}/api/v1/Admin/product/preview?productImage=productImage_{newProduct.ProductId}_{productDto.Files[0]?.FileName}";
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
                    }

                    ;
                    listProductImage.Add(new ProductImage()
                    {
                        Image = file.FileName,
                        Url =
                            $"{request.Scheme}://{request.Host}/api/v1/Admin/product/preview?productImage=productImage_{newProduct.ProductId}_{file.FileName}",
                        ProductId = newProduct.ProductId,
                        CreatedAt = DateTime.Now,
                        Product = newProduct,
                        ProductImageId = Guid.NewGuid(),
                    });
                }

                var listColor = new List<ProductColor>();

                foreach (var color in Helpers.ParseString<string>(productDto.ColorCode))
                {
                    listColor.Add(new ProductColor()
                    {
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
            catch (DbUpdateException ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<Product> UpdateProductAsync(ProductDto productDto, Guid productId, string userName,
            HttpRequest request, CancellationToken userCancellationToken)
        {
            try
            {
                var updateProduct = await _context
                                        .Products
                                        .Where(p => p.ProductId == productId)
                                        .Include(p => p.ProductImages)
                                        .Include(p => p.ProductColors)
                                        .FirstOrDefaultAsync(userCancellationToken)
                                    ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Product not found.");

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
                        await _cloudflareClient.DeleteObjectAsync(
                            $"productImage_{updateProduct.ProductId}_{image.Image}", userCancellationToken);
                        _context.ProductImages.Remove(image);
                    }
                }

                if (listAddImage.Count > 0)
                {
                    foreach (var imageFile in listAddImage)
                    {
                        if (imageFile.FileName != updateProduct.Image)
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
                                Url =
                                    $"{request.Scheme}://{request.Host}/api/v1/Admin/product/preview?productImage=productImage_{updateProduct.ProductId}_{imageFile.FileName}",
                                CreatedAt = DateTime.Now,
                                ModifiedAt = DateTime.Now,
                            }, userCancellationToken);
                        }

                        if (imageFile.FileName == updateProduct.Image && imageFile.FileName != listNewImage[0])
                        {
                            await _context.ProductImages.AddAsync(new ProductImage()
                            {
                                ProductImageId = Guid.NewGuid(),
                                Product = updateProduct,
                                ProductId = updateProduct.ProductId,
                                Image = imageFile.FileName,
                                Url =
                                    $"{request.Scheme}://{request.Host}/api/v1/Admin/product/preview?productImage=productImage_{updateProduct.ProductId}_{imageFile.FileName}",
                                CreatedAt = DateTime.Now,
                                ModifiedAt = DateTime.Now,
                            }, userCancellationToken);
                        }
                    }
                }

                if (!listNewImage.Contains(updateProduct.Image))
                {
                    await _cloudflareClient.DeleteObjectAsync(
                        $"productImage_{updateProduct.ProductId}_{updateProduct.Image}", userCancellationToken);
                    updateProduct.Image = listNewImage[0];
                    updateProduct.Url =
                        $"{request.Scheme}://{request.Host}/api/v1/Admin/product/preview?productImage=productImage_{updateProduct.ProductId}_{listNewImage[0]}";
                    _context.ProductImages.Remove(
                        updateProduct.ProductImages.Where(pi => pi.Image == updateProduct.Image).FirstOrDefault() ??
                        throw new HttpStatusException(HttpStatusCode.NotFound,
                            "Can't remove image because product not found."));
                }

                if (listNewImage.Contains(updateProduct.Image) && listNewImage[0] != updateProduct.Image)
                {
                    updateProduct.Image = listNewImage[0];
                    updateProduct.Url =
                        $"{request.Scheme}://{request.Host}/api/v1/Admin/product/preview?productImage=productImage_{updateProduct.ProductId}_{listNewImage[0]}";
                    _context.ProductImages.Remove(
                        updateProduct.ProductImages.Where(pi => pi.Image == updateProduct.Image).FirstOrDefault() ??
                        throw new HttpStatusException(HttpStatusCode.NotFound,
                            "Can't remove image because product not found."));
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

                foreach (var color in listAddColor)
                {
                    await _context.ProductColors.AddAsync(new ProductColor()
                    {
                        ColorId = Guid.NewGuid(),
                        ColorCode = color,
                        Product = updateProduct,
                        ProductId = updateProduct.ProductId,
                    }, userCancellationToken);
                }

                await _context.SaveChangesAsync(userCancellationToken);

                return updateProduct;
            }
            catch (SqlException ex)
            {
                throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
            }
        }

        public async Task<FileStreamResult> GetImageAsync(string imageUrl, CancellationToken userCancellationToken)
        {
            var response = await _cloudflareClient.GetObjectAsync(imageUrl, userCancellationToken);
            if (response.HttpStatusCode == HttpStatusCode.OK)
            {
                return new FileStreamResult(response.ResponseStream, response.Headers.ContentType)
                {
                    FileDownloadName = imageUrl
                };
            }

            throw new HttpStatusException(response.HttpStatusCode, "Image not found.");
        }
    }
}