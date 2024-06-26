using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EcommerceApi.Models.Product;

public class Product
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [JsonPropertyName("id")]
    [Key] public Guid ProductId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Discount { get; set; }
    public bool Status { get; set; }
    public bool Hot { get; set; }
    public bool FlashSale { get; set; }
    public bool Upcoming { get; set; }
    public int Return { get; set; }
    public string Image { get; set; } = string.Empty;
    [DataType(DataType.ImageUrl)] public string Url { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public ProductStock ProductStock { get; set; }
    [ForeignKey("CategoryId")]
    [JsonIgnore]
    public ProductCategory ProductCategory { get; set; } = new ProductCategory();
    public List<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    public List<ProductColor> ProductColors { get; set; } = new List<ProductColor>();
    public List<Rate.Rate> ProductRates { get; set; } = new List<Rate.Rate>();
}