using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApi.Models.Coupon;

public class Condition
{
    [Key] public int ConditionId { get; set; }
    public string Attribute { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
}