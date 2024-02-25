using EcommerceApi.Models.Product;

namespace EcommerceApi.Responses;

public class ProductCategoryResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Status { get; set; }
    public bool Hot { get; set; }
    public List<ProductCategory> ListProductCategoryChild { get; set; } = new();
    public ProductResponse Product { get; set; } = new();
    public int? ParentCategoryId { get; set; }
}