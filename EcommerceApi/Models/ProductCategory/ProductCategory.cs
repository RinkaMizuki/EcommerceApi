using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EcommerceApi.Models.Product;

public class ProductCategory
{
    [JsonPropertyName("id")]
    [Key] public int CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool Status { get; set; }
    public bool Hot { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;

    public int? ParentCategoryId { get; set; }

    [ForeignKey("ParentCategoryId")]
    [JsonIgnore]
    public ProductCategory ParentProductCategory { get; set; } = null;

    public List<Product> Products { get; set; } = new();
    public List<ProductCategory> ListProductCategoryChild { get; set; } = new();
}