using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.Models.Segment;

public class Segment
{
    [Key]public int SegmentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public List<UserSegment> Users { get; set; } = new List<UserSegment> ();
}