using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApi.Models.Order;

public class OrderDetail
{
    [Key]
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    [ForeignKey("ProductId")]
    public Product.Product Proclearduct { get; set; }
    [ForeignKey("OrderId")] public Order Order { get; set; }
    public int DiscountProduct { get; set; }
    public int PriceProduct { get; set; }
    public string Note { get; set; } = string.Empty;
    public bool Confirm { get; set; } = false;
    public Guid Token { get; set; }
}