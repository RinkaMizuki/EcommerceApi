using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using EcommerceApi.Models.Order;

namespace EcommerceApi.Models.Product;

public class Product
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key] public Guid ProductId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Discount { get; set; }
    public Boolean Status { get; set; }
    public Boolean Hot { get; set; }
    public Boolean FlashSale { get; set; }
    public Boolean Upcoming { get; set; }
    public int Quantity { get; set; }
    public int Return { get; set; }
    public string Image { get; set; } = string.Empty;
    [DataType(DataType.ImageUrl)] public string Url { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    [ForeignKey("CategoryId")]
    [JsonIgnore]
    public ProductCategory ProductCategory { get; set; }
    public List<ProductImage> ProductImages { get; set; }
    public List<ProductColor> ProductColors { get; set; }
}