using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApi.Models.Product;

public class ProductColor
{
    [Key] public Guid ColorId { get; set; }
    public string ColorCode { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    [ForeignKey("ProductId")]
    public Product Product { get; set; }
}