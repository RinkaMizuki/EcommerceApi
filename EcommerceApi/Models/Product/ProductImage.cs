using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApi.Models.Product;

public class ProductImage
{
    [Key] public Guid ProductImageId { get; set; }
    public Guid ProductId { get; set; }
    [ForeignKey("ProductId")]
    public Product Product { get; set; }

    public string Image { get; set; } = string.Empty;
    [DataType(DataType.ImageUrl)] public string Url { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
}