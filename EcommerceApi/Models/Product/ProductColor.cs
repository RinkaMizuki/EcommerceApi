using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EcommerceApi.Models.Product;

public class ProductColor
{
    [Key] public Guid ColorId { get; set; }
    public string ColorCode { get; set; } = string.Empty;
    [JsonIgnore]
    public Guid ProductId { get; set; }
    [ForeignKey("ProductId")]
    [JsonIgnore]
    public Product Product { get; set; }
}