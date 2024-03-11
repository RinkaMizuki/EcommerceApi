using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using EcommerceApi.Models.UserAddress;

namespace EcommerceApi.Models.Segment;

public class UserSegment
{
    [JsonIgnore]
    public int UserId { get; set; }
    [ForeignKey("UserId")]

    [JsonIgnore]
    public User User { get; set; }
    [JsonIgnore]
    public int SegmentId { get; set; }
    [ForeignKey("SegmentId")]
    public Segment Segment { get; set; }
}