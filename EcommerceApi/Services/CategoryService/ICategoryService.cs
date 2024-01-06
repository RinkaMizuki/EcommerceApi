using EcommerceApi.Dtos.Admin;
using EcommerceApi.Models.Product;

namespace EcommerceApi.Services.CategoryService;

public interface ICategoryService
{
    public Task<List<ProductCategory>> GetListCategoryAsync();
    public List<ProductCategory> FakeCategory();
    public Task<ProductCategory> PostCategoryAsync(CategoryDto categoryDto, string userName);
    public Task<Boolean> DeleteCategoryAsync(int categoryId);
    public Task<ProductCategory> UpdateCategoryAsync(CategoryDto categoryDto, int categoryId, string userName);
    public Task<ProductCategory?> GetCategoryByIdAsync(int categoryId);
}