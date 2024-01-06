using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApi.Models.Segment;

public class UserSegment
{
    public int UserId { get; set; }
    [ForeignKey("UserId")] public User User { get; set; }
    public int SegmentId { get; set; }
    [ForeignKey("SegmentId")] public Segment Segment { get; set; }
}