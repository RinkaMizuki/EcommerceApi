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
//using SortOrder = EcommerceApi.Constant.SortOrder;
//using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using EcommerceApi.FilterBuilder;
using k8s.KubeConfigModels;

namespace EcommerceApi.Services.ProductService
{
    public class ProductService : IProductService
    {
        private readonly EcommerceDbContext _context;
        private readonly ICloudflareClientService _cloudflareClient;
        private readonly ProductFilterBuilder _productFilter;
        public ProductService(EcommerceDbContext context, ICloudflareClientService cloudflareClient, ProductFilterBuilder productFilterBuilder)
        {
            _context = context;
            _cloudflareClient = cloudflareClient;
            _productFilter = productFilterBuilder;
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
                    rangeValues.AddRange(new List<int> { 0, 11 });
                }

                var sortValues = Helpers.ParseString<string>(sort);

                if (sortValues.Count == 0)
                {
                    sortValues.AddRange(new List<string> { "", "" });
                }

                var filterValues = Helpers.ParseString<string>(filter);
                var sortString = string.Join(", ", sortValues.Where((s, i) => i % 2 == 0)
                                           .Zip(sortValues.Where((s, i) => i % 2 != 0), (a, b) => $"{(a == "id" ? "productId" : a)} {b}")).Trim();

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

                if (!filterValues.Contains(ProductFilterType.Suggest))
                {
                    filterValues.Add(ProductFilterType.Suggest);
                    filterValues.Add("");
                }

                if (!filterValues.Contains(ProductFilterType.Random))
                {
                    filterValues.Add(ProductFilterType.Random);
                    filterValues.Add("8");
                }

                if(!filterValues.Contains(ProductFilterType.Total))
                {
                    filterValues.Add(ProductFilterType.Total);
                    filterValues.Add("-1");
                }

                if (!filterValues.Contains(ProductFilterType.Favorite))
                {
                    filterValues.Add(ProductFilterType.Favorite);
                    filterValues.Add("");
                }
                
                if(!filterValues.Contains(ProductFilterType.PriceRange))
                {
                    filterValues.Add(ProductFilterType.PriceRange);
                    filterValues.Add("");
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
                var priceRange = filterValues
                    .Skip(filterValues.IndexOf(ProductFilterType.PriceRange) + 1)
                    .Take(2)
                    .ToList();

                var perPage = rangeValues[1] - rangeValues[0] + 1;
                var currentPage = Convert.ToInt32(Math.Ceiling((double)rangeValues[0] / perPage)) + 1;

                var listProductQuery = _context
                    .Products
                    .Include(p => p.ProductStock)
                    .Include(p => p.ProductImages)
                    .Include(p => p.ProductColors)
                    .Include(p => p.ProductRates)
                    .Include(p => p.ProductCategory)
                    .ThenInclude(pc => pc.ParentProductCategory);

                var priceMaxDefault = Convert.ToInt32(listProductQuery.Max(p => p.Price));
                //conditions filter
                var minStock = string.IsNullOrEmpty(stockRange[0]) ? -999 : Convert.ToInt32(stockRange[0]);
                var maxStock = string.IsNullOrEmpty(stockRange[1]) ? -999 : Convert.ToInt32(stockRange[1]);
                var minPrice = string.IsNullOrEmpty(priceRange[0]) ? 0 : Convert.ToInt32(priceRange[0]);
                var maxPrice = string.IsNullOrEmpty(priceRange[1]) ? priceMaxDefault : Convert.ToInt32(priceRange[1]);

                var category = filterValues[filterValues.IndexOf(ProductFilterType.Category) + 1].ToString().ToLower();
                var searchValue = filterValues[filterValues.IndexOf(ProductFilterType.Search) + 1].ToLower();
                var sale = filterValues[filterValues.IndexOf(ProductFilterType.Sale) + 1].ToLower();
                var forYou = filterValues[filterValues.IndexOf(ProductFilterType.Suggest) + 1].ToLower();
                var numberRandom = Convert.ToInt32(filterValues[filterValues.IndexOf(ProductFilterType.Random) + 1].ToLower());
                var favorites = new List<Guid>();


                var skipElm = filterValues
                    .IndexOf(ProductFilterType.Favorite) + 1;
                var takeElm = Convert.ToInt32(filterValues[filterValues.IndexOf(ProductFilterType.Total) + 1]);

                if (!string.IsNullOrEmpty(filterValues[filterValues.IndexOf(ProductFilterType.Favorite) + 1])
                    && filterValues[filterValues.IndexOf(ProductFilterType.Total) + 1] != "0")
                {
                    favorites = filterValues
                    .Skip(skipElm)
                    .Take(takeElm)
                    .Select(id => new Guid(id))
                    .ToList();
                }

                var listProduct = await listProductQuery
                    .AsNoTracking()
                    .ToListAsync(userCancellationToken);

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

                var filters = _productFilter
                                            .AddPriceSaleFilter(minPrice, maxPrice)
                                            .AddSearchFilter(searchValue)
                                            .AddSaleFilter(sale)
                                            .AddCategoryFilter(category)
                                            .AddQuantityFilter(minStock, maxStock)
                                            .Build();

                listProduct = listProduct
                                         .Where(filters).ToList();

                if(favorites.Count > 0)
                {
                    listProduct = listProduct
                                            .Where(p => favorites.Contains(p.ProductId))
                                            .ToList();
                }
                else if(takeElm == 0)
                {
                    listProduct = new();
                }

                if (!string.IsNullOrEmpty(forYou)) {
                    listProduct = Helpers.GetRandomElements(listProduct, numberRandom);
                }
                if(!string.IsNullOrEmpty(sortString))
                {
                    listProduct = listProduct.AsQueryable().OrderBy(sortString).ToList();
                }

                {
                    //var totalProduct = listProduct.Count;

                    //listProduct = listProduct
                    //    .Skip((currentPage - 1) * perPage)
                    //    .Take(perPage).ToList();

                    //response.Headers.Append("Access-Control-Expose-Headers", "Content-Range");
                    //response.Headers.Append("Content-Range", $"products {rangeValues[0]}-{rangeValues[1]}/{totalProduct}");

                    //return listProduct;
                }
                var listProductPaging = Helpers.CreatePaging(listProduct, rangeValues, currentPage, perPage, "products", response);
                return listProductPaging;
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
                                      .Include(p => p.ProductStock)
                                      .Include(p => p.ProductImages)
                                      .Include(p => p.ProductColors)
                                      .Include(p => p.ProductRates)
                                      .ThenInclude(pr => pr.FeedbackRate)
                                      .AsNoTracking()
                                      .FirstOrDefaultAsync(userCancellationToken)
                                  ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Product not found.");

                return productById;
            }
            catch (Exception ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<Product> PostProductAsync(ProductDto productDto, string userName, HttpRequest request,
            CancellationToken userCancellationToken)
        {
            using var productTransaction = await _context.Database.BeginTransactionAsync(userCancellationToken);
            var category =
                    await _context.ProductCategories.FindAsync(new object?[] { productDto.CategoryId },
                        cancellationToken: userCancellationToken) ??
                    throw new HttpStatusException(HttpStatusCode.NotFound, "Category not found.");
            try
            {
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

                List<ProductImage> listProductImage = new ();
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
                        Url =
                            $"{request.Scheme}://{request.Host}/api/v1/Admin/product/preview?productImage=productImage_{newProduct.ProductId}_{file.FileName}",
                        ProductId = newProduct.ProductId,
                        CreatedAt = DateTime.Now,
                        Product = newProduct,
                        ProductImageId = Guid.NewGuid(),
                    });
                }

                var listColor = new List<ProductColor>();
                var listNewColor = productDto.ColorCode.Split(',');
                foreach (var color in listNewColor)
                {
                    listColor.Add(new ProductColor()
                    {
                        ProductId = newProduct.ProductId,
                        ColorCode = color,
                        ColorId = Guid.NewGuid(),
                        Product = newProduct,
                    });
                }
                var productStock = new ProductStock()
                {
                    ProductId = newProduct.ProductId,
                    Product = newProduct,
                    StockQuantity = productDto.Quantity,
                    Location = productDto.Location
                };

                await _context
                            .ProductStocks
                            .AddAsync(productStock, userCancellationToken);
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

                await productTransaction.CommitAsync(userCancellationToken);
                return newProduct;
            }
            catch (DbUpdateException ex)
            {
                await productTransaction.RollbackAsync(userCancellationToken);
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<Product> UpdateProductAsync(ProductDto productDto, Guid productId, string userName,
            HttpRequest request, CancellationToken userCancellationToken)
        {
            using var productTransaction = await _context.Database.BeginTransactionAsync(userCancellationToken); 
            try
            {
                var updateProduct = await _context
                                        .Products
                                        .Where(p => p.ProductId == productId)
                                        .Include(p => p.ProductStock)
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
                updateProduct.Price = productDto.Price;
                updateProduct.Status = productDto.Status;
                updateProduct.Discount = productDto.Discount;
                updateProduct.Hot = productDto.Hot;
                updateProduct.FlashSale = productDto.FlashSale;
                updateProduct.Upcoming = productDto.Upcoming;
                updateProduct.Return = productDto.Return;
                updateProduct.CategoryId = productDto.CategoryId;

                string[] listNewColor = Array.Empty<string>();
                List<string> listOldColor = new();
                List<ProductColor> listDeleteColor = new();
                List<string> listAddColor = new();

                if (productDto.ColorCode is not null)
                {
                     listNewColor = productDto.ColorCode.Split(',');
                     listOldColor = updateProduct
                                                .ProductColors
                                                .Select(pc => pc.ColorCode)
                                                .ToList();
                     listDeleteColor = updateProduct
                        .ProductColors
                        .Where(pc => !listNewColor.Contains(pc.ColorCode) && productId == updateProduct.ProductId)
                        .Select(pc => pc)
                        .ToList();
                     listAddColor = listNewColor
                        .Where(i => !listOldColor.Contains(i) && productId == updateProduct.ProductId)
                        .ToList();
                }


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

                if(updateProduct.ProductStock != null)
                {
                    updateProduct.ProductStock.StockQuantity = productDto.Quantity;
                    updateProduct.ProductStock.Location = productDto.Location;
                }

                await _context.SaveChangesAsync(userCancellationToken);

                await productTransaction.CommitAsync(userCancellationToken);

                return updateProduct;
            }
            catch (Exception ex)
            {
                await productTransaction.RollbackAsync(userCancellationToken);
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
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