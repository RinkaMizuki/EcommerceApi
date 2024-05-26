using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EcommerceApi.Models.Order;

public class OrderDetail
{
    [Key]
    public Guid OrderId { get; set; }
    [Key]
    public Guid ProductId { get; set; }
    [ForeignKey("ProductId")]
    public Product.Product Product { get; set; }
    [JsonIgnore]
    [ForeignKey("OrderId")]
    public Order Order { get; set; }
    public string Color { get; set; } = string.Empty;
    public int DiscountProduct { get; set; }
    public int PriceProduct { get; set; }
    public int QuantityProduct { get; set; }
}