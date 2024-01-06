namespace EcommerceApi.Dtos.Admin;

public class CategoryDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Boolean Status { get; set; }
    public Boolean Hot { get; set; }
    public int? ParentCategoryId { get; set; }
}