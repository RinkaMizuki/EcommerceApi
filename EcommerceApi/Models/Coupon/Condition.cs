using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EcommerceApi.Models.Coupon;

public class Condition
{
    [JsonIgnore]
    [Key] public Guid ConditionId { get; set; }
    public string Attribute { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
}