using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.Models.Coupon;

public class Coupon
{
    [Key]public int CouponId { get; set; }
    public string CouponCode { get; set; } = string.Empty;
    public Boolean IsActive { get; set; } = true;
    public int DiscountPercent { get; set; }
}