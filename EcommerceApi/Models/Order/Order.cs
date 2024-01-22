using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using EcommerceApi.Models.Segment;

namespace EcommerceApi.Models.Order;

public class Order
{
    [Key] public Guid OrderId { get; set; }
    
    public int UserId { get; set; }
    [JsonIgnore]
    [ForeignKey("UserId")] 
    public User User { get; set; }
    
    public Guid CouponId { get; set; }
    [ForeignKey("CouponId")] public Coupon.Coupon Coupon { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime DeliveredDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalDiscount { get; set; }
    public int TotalQuantity { get; set; }
    public decimal TotalPrice { get; set; }
    public bool Returned { get; set; }
    public bool Confirm { get; set; } = false;
    public Guid Token { get; set; }
    public List<OrderDetail> OrderDetails { get; set; } = new();
}