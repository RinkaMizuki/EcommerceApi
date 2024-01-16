using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EcommerceApi.Models.Order;

public class OrderDetail
{
    [Key]
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    [ForeignKey("ProductId")]
    public Product.Product Product { get; set; }
    [JsonIgnore]
    [ForeignKey("OrderId")]
    public Order Order { get; set; }
    public int DiscountProduct { get; set; }
    public int PriceProduct { get; set; }
    public string Note { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string DeliveryAddress {  get; set; } = string.Empty;
}