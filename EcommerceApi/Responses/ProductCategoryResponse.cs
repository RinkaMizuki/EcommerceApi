namespace EcommerceApi.Responses;

public class ProductCategoryResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Boolean Status { get; set; }
    public Boolean Hot { get; set; }
    public List<ProductCategoryResponse> ListProductCategoryChild { get; set; } = new();
    public ProductResponse Product { get; set; } = new();
}