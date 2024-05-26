using EcommerceApi.Dtos.Admin;
using EcommerceApi.ExtensionExceptions;
using EcommerceApi.Models;
using EcommerceApi.Models.Product;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Net;
using EcommerceApi.Constant;

namespace EcommerceApi.Services.CategoryService;

public class CategoryService : ICategoryService
{
    private readonly EcommerceDbContext _context;

    public CategoryService(EcommerceDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductCategory>> GetListCategoryAsync(string filter,
        CancellationToken userCancellationToken)
    {
        try
        {
            // Step 1: Load root categories
            var listCate = await _context.ProductCategories
                .Where(pc => pc.ParentCategoryId == null) // Assuming root categories have ParentCategoryId as null
                .Include(pc => pc.Products)
                .AsNoTracking()
                .ToListAsync(userCancellationToken);

            // Step 2: Recursively load child categories
            foreach (var rootCategory in listCate)
            {
                await LoadChildCategories(rootCategory, userCancellationToken);
            }

            var filterValues = Helpers.ParseString<string>(filter);

            if (filterValues.Contains(CategoryFilterType.Key))
            {
                var keyStartIndex = filterValues.IndexOf(CategoryFilterType.Key);
                var listId = filterValues
                    .Skip(keyStartIndex + 1)
                    .Take(filterValues.Count - 1)
                    .Select(int.Parse).ToList();
                var allCategories = listCate.Concat(listCate.SelectMany(c => c.ListProductCategoryChild)).ToList();
                listCate = allCategories
                    .Where(c => listId.Contains(c.CategoryId))
                    .ToList();
            }
            else if(filterValues.Contains(CategoryFilterType.All))
            {
                var allCategories = listCate.Concat(listCate.SelectMany(c => c.ListProductCategoryChild)).ToList();
                return allCategories;
            }
            else
            {
                listCate = listCate.Where(c => c.ParentCategoryId == null).ToList();
            }
 
            return listCate;
        }
        catch (Exception ex)
        {
            throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    public async Task<ProductCategory> PostCategoryAsync(CategoryDto categoryDto, string userName,
        CancellationToken userCancellationToken)
    {
        try
        {
            ProductCategory? parentCate =
                await _context.ProductCategories.FindAsync(categoryDto.ParentCategoryId, userCancellationToken);
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

    private async Task LoadChildCategories(ProductCategory category, CancellationToken userCancellationToken)
    {
        category.ListProductCategoryChild = await _context.ProductCategories
            .Where(pc => pc.ParentCategoryId == category.CategoryId) // Assuming there's a ParentCategoryId to reference the parent
            .Include(pc => pc.Products)
            .AsNoTracking()
            .ToListAsync(userCancellationToken);

        foreach (var childCategory in category.ListProductCategoryChild)
        {
            await LoadChildCategories(childCategory, userCancellationToken);
        }
    }

}