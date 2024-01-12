using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EcommerceApi.Models.Coupon;

public class CouponCondition
{
    [JsonIgnore]
    public Guid CouponId { get; set; }
    public decimal Value { get; set; } 
    public Guid ConditionId { get; set; }
    [JsonIgnore]
    [ForeignKey("CouponId")] 
    public Coupon Coupon { get; set; }
    [ForeignKey("ConditionId")] 
    public Condition Condition { get; set; }
}