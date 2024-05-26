using EcommerceApi.Dtos.Admin;
using EcommerceApi.Models.Product;
using EcommerceApi.Responses;

namespace EcommerceApi.Services.CategoryService;

public interface ICategoryService
{
    public Task<List<ProductCategory>> GetListCategoryAsync(string filter,
        CancellationToken userCancellationToken);

    public Task<ProductCategory> PostCategoryAsync(CategoryDto categoryDto, string userName,
        CancellationToken userCancellationToken);

    public Task<Boolean> DeleteCategoryAsync(int categoryId, CancellationToken userCancellationToken);

    public Task<ProductCategory> UpdateCategoryAsync(CategoryDto categoryDto, int categoryId, string userName,
        CancellationToken userCancellationToken);

    public Task<ProductCategory?> GetCategoryByIdAsync(int categoryId, CancellationToken userCancellationToken);
}