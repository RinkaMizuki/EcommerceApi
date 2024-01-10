using EcommerceApi.Dtos.Admin;
using EcommerceApi.Models.Product;

namespace EcommerceApi.Services.CategoryService;

public interface ICategoryService
{
    public Task<List<ProductCategory>> GetListCategoryAsync(CancellationToken userCancellationToken);
    public List<ProductCategory> FakeCategory();
    public Task<ProductCategory> PostCategoryAsync(CategoryDto categoryDto, string userName, CancellationToken userCancellationToken);
    public Task<Boolean> DeleteCategoryAsync(int categoryId, CancellationToken userCancellationToken);
    public Task<ProductCategory> UpdateCategoryAsync(CategoryDto categoryDto, int categoryId, string userName, CancellationToken userCancellationToken);
    public Task<ProductCategory?> GetCategoryByIdAsync(int categoryId, CancellationToken userCancellationToken);
}