namespace EcommerceApi.Dtos.Admin;

public class CategoryDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Status { get; set; }
    public bool Hot { get; set; }
    public int? ParentCategoryId { get; set; }
}