using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcommerceApi.Models.Segment;

namespace EcommerceApi.Models.Order;

public class Order
{
    [Key] public Guid OrderId { get; set; }
    public int UserId { get; set; }
    [ForeignKey("UserId")] public User User { get; set; }
    public Guid CouponId { get; set; }
    [ForeignKey("CouponId")] public Coupon.Coupon Coupon { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime DeliveredDate { get; set; }
    public bool Delivered { get; set; }
    public bool Status { get; set; }
    public decimal TotalDiscount { get; set; }
    public int TotalQuantity { get; set; }
}