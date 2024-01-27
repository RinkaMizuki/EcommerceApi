using EcommerceApi.Dtos.Admin;
using EcommerceApi.ExtensionExceptions;
using EcommerceApi.Models;
using EcommerceApi.Models.Product;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Net;
using EcommerceApi.Responses;

namespace EcommerceApi.Services.CategoryService;

public class CategoryService : ICategoryService
{
    private readonly EcommerceDbContext _context;

    public CategoryService(EcommerceDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductCategoryResponse>> GetListCategoryAsync(CancellationToken userCancellationToken)
    {
        //var listFakeCate = FakeCategory();
        //await _context.ProductCategories.AddRangeAsync(listFakeCate);
        //await _context.SaveChangesAsync();
        try
        {
            var listCate = await _context
                .ProductCategories
                .Where(pc => pc.ParentProductCategory == null)
                .Select(pc => new ProductCategoryResponse()
                {
                    Id = pc.CategoryId,
                    Title = pc.Title,
                    Description = pc.Description,
                    Status = pc.Status,
                    Hot = pc.Hot,
                    Product = pc.Products
                        .Select(p => new ProductResponse()
                        {
                            Image = p.Image,
                            Url = p.Url,
                        }).FirstOrDefault()!,
                })
                .AsNoTracking()
                .ToListAsync(userCancellationToken);
            return listCate;
        }
        catch (SqlException ex)
        {
            throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
        }
    }

    public List<ProductCategory> FakeCategory()
    {
        try
        {
            var laptop = new ProductCategory()
            {
                Title = "Laptop",
                Description = "Đây là danh mục laptop",
                Status = true,
                Hot = false,
                CreatedAt = DateTime.Now,
                ModifiedAt = DateTime.Now,
                ModifiedBy = string.Empty,
                CreatedBy = string.Empty,
                ParentProductCategory = null,
            };
            var listFakeCate = new List<ProductCategory>
            {
                new ProductCategory()
                {
                    Title = "Laptop gaming",
                    Description = "Đây là danh mục laptop gaming",
                    Status = true,
                    Hot = false,
                    CreatedAt = DateTime.Now,
                    ModifiedAt = DateTime.Now,
                    ModifiedBy = string.Empty,
                    CreatedBy = string.Empty,
                    ParentProductCategory = laptop,
                },
                new ProductCategory()
                {
                    Title = "PC",
                    Description = "Đây là danh mục pc",
                    Status = true,
                    Hot = false,
                    CreatedAt = DateTime.Now,
                    ModifiedAt = DateTime.Now,
                    ModifiedBy = string.Empty,
                    CreatedBy = string.Empty,
                    ParentProductCategory = null,
                },
                new ProductCategory()
                {
                    Title = "Mobile",
                    Description = "Đây là danh mục mobile",
                    Status = true,
                    Hot = false,
                    CreatedAt = DateTime.Now,
                    ModifiedAt = DateTime.Now,
                    ModifiedBy = string.Empty,
                    CreatedBy = string.Empty,
                    ParentProductCategory = null,
                },
            };
            return listFakeCate;
        }
        catch (SqlException ex)
        {
            throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
        }
    }
    public async Task<ProductCategory> PostCategoryAsync(CategoryDto categoryDto, string userName,
        CancellationToken userCancellationToken)
    {
        try
        {
            ProductCategory? parentCate = await _context.ProductCategories.FindAsync(categoryDto.ParentCategoryId);
            var newCategory = new ProductCategory()
            {
                Title = categoryDto.Title,
                Description = categoryDto.Description,
                Status = categoryDto.Status,
                Hot = categoryDto.Hot,
                ParentCategoryId = Convert.ToInt32(categoryDto.ParentCategoryId) == -1
                    ? null
                    : Convert.ToInt32(categoryDto.ParentCategoryId),
                ParentProductCategory = parentCate,
                CreatedAt = DateTime.Now,
                ModifiedAt = DateTime.Now,
                CreatedBy = userName,
                ModifiedBy = userName,
            };
            await _context.ProductCategories.AddAsync(newCategory, userCancellationToken);
            await _context.SaveChangesAsync(userCancellationToken);
            return newCategory;
        }
        catch (SqlException ex)
        {
            throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
        }
    }

    public async Task<bool> DeleteCategoryAsync(int categoryId, CancellationToken userCancellationToken)
    {
        try
        {
            var deletedCate =
                await _context.ProductCategories.FindAsync(new object[] { categoryId }, userCancellationToken) ??
                throw new HttpStatusException(HttpStatusCode.NotFound, "Category not found.");
            _context.ProductCategories.Remove(deletedCate);
            await _context.SaveChangesAsync(userCancellationToken);
            return true;
        }
        catch (SqlException ex)
        {
            throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
        }
    }

    public async Task<ProductCategory> UpdateCategoryAsync(CategoryDto categoryDto, int categoryId, string userName,
        CancellationToken userCancellationToken)
    {
        try
        {
            var updateCategory = await _context.ProductCategories
                                     .Where(cate => cate.CategoryId == categoryId)
                                     .Include(c => c.ListProductCategoryChild)
                                     .Include(c => c.ParentProductCategory)
                                     .FirstOrDefaultAsync(userCancellationToken)
                                 ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Category not found.");

            if (Convert.ToInt32(categoryDto.ParentCategoryId) == -1)
            {
                foreach (var childCate in updateCategory.ListProductCategoryChild)
                {
                    if (updateCategory.ParentCategoryId != null)
                    {
                        childCate.ParentCategoryId = updateCategory.ParentCategoryId;
                        childCate.ParentProductCategory = updateCategory.ParentProductCategory;
                    }
                }

                updateCategory.ParentCategoryId = null;
            }
            else if (updateCategory.ListProductCategoryChild
                         .Where(cateChild => cateChild.CategoryId == categoryDto.ParentCategoryId).Any() ||
                     categoryDto.ParentCategoryId == updateCategory.CategoryId)
            {
                throw new HttpStatusException(HttpStatusCode.BadRequest, "Can't update category");
            }
            else
            {
                foreach (var childCate in updateCategory.ListProductCategoryChild)
                {
                    if (updateCategory.ParentCategoryId != null)
                    {
                        childCate.ParentCategoryId = updateCategory.ParentCategoryId;
                        childCate.ParentProductCategory = updateCategory.ParentProductCategory;
                    }
                }

                updateCategory.ParentCategoryId = categoryDto.ParentCategoryId;
            }

            updateCategory.Title = categoryDto.Title;
            updateCategory.Description = categoryDto.Description;
            updateCategory.Status = categoryDto.Status;
            updateCategory.Hot = categoryDto.Hot;
            updateCategory.ModifiedAt = DateTime.Now;
            updateCategory.ModifiedBy = userName;

            await _context.SaveChangesAsync(userCancellationToken);

            return updateCategory;
        }
        catch (SqlException ex)
        {
            throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
        }
    }

    public async Task<ProductCategory?> GetCategoryByIdAsync(int categoryId, CancellationToken userCancellationToken)
    {
        try
        {
            var cateById = await _context
                               .ProductCategories
                               .Include(pc => pc.ListProductCategoryChild)
                               .ThenInclude(pc => pc.Products)
                               .Where(pc => pc.CategoryId == categoryId)
                               .FirstOrDefaultAsync(userCancellationToken)
                           ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Category not found.");
            return cateById;
        }
        catch (SqlException ex)
        {
            throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
        }
    }
}