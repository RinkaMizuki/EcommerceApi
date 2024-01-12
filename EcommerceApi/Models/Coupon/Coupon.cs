using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EcommerceApi.Models.Coupon;

public class Coupon
{
    [Key]public Guid CouponId { get; set; }
    public string CouponCode { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int DiscountPercent { get; set; }
    public List<CouponCondition> CouponConditions { get; set; } = new List<CouponCondition>();
}