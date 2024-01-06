using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApi.Models.Coupon;

public class CouponCondition
{
    public int CouponId { get; set; }
    public decimal Value { get; set; } 
    public int ConditionId { get; set; }
    [ForeignKey("CouponId")] public Coupon Coupon { get; set; }
    [ForeignKey("ConditionId")] public Condition Condition { get; set; }
}