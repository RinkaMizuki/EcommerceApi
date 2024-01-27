using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using EcommerceApi.Models.Feedback;
using EcommerceApi.Models.Segment;

namespace EcommerceApi.Models.Rate;

public class Rate
{
    [JsonPropertyName("id")]
    [Key]public int RateId { get; set; }
    public Guid ProductId { get; set; }
    [ForeignKey("ProductId")]
    [JsonIgnore]
    public Product.Product Product { get; set; }
    public int UserId { get; set; }
    [ForeignKey("UserId")]
    [JsonIgnore]
    public User User { get; set; }
    public string Content { get; set; } = string.Empty;
    public int Star { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public FeedbackRate FeedbackRate { get; set; }
}