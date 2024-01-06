using EcommerceApi.Dtos.Admin;
using EcommerceApi.Models;
using EcommerceApi.Models.Product;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApi.Services.CategoryService;

public class CategoryService : ICategoryService
{
    private readonly EcommerceDbContext _context;

    public CategoryService(EcommerceDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductCategory>> GetListCategoryAsync()
    {
        //var listFakeCate = FakeCategory();
        //await _context.ProductCategories.AddRangeAsync(listFakeCate);
        //await _context.SaveChangesAsync();
        var listCate = await _context.ProductCategories
            .ToListAsync();
        return listCate;
    }

    public List<ProductCategory> FakeCategory()
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
    
    public async Task<ProductCategory> PostCategoryAsync(CategoryDto categoryDto, string userName)
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
        await _context.ProductCategories.AddAsync(newCategory);
        await _context.SaveChangesAsync();
        return newCategory;
    }

    public async Task<bool> DeleteCategoryAsync(int categoryId)
    {
        var deletedCate = await _context.ProductCategories.FindAsync(categoryId);
        if (deletedCate == null)
        {
            return false;
        }

        _context.ProductCategories.Remove(deletedCate);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ProductCategory> UpdateCategoryAsync(CategoryDto categoryDto, int categoryId, string userName)
    {
        var updateCategory = await _context.ProductCategories
                                            .Where(cate => cate.CategoryId == categoryId)
                                            .Include(c => c.ListProductCategoryChild)
                                            .Include(c => c.ParentProductCategory)
                                            .FirstOrDefaultAsync();
        if(updateCategory != null) { 
            if(Convert.ToInt32(categoryDto.ParentCategoryId) == -1)
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
            else if(updateCategory.ListProductCategoryChild.Where(cateChild => cateChild.CategoryId == categoryDto.ParentCategoryId).Any() || categoryDto.ParentCategoryId == updateCategory.CategoryId)
            {
                return null;
            }
            else
            {
                foreach (var childCate in updateCategory.ListProductCategoryChild)
                {
                    if(updateCategory.ParentCategoryId != null)
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
            await _context.SaveChangesAsync();
        }
        return updateCategory;
    }

    public async Task<ProductCategory?> GetCategoryByIdAsync(int categoryId)
    {
        var cateById = await _context.ProductCategories.FindAsync(categoryId);
        return cateById;
    }
}