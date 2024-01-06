using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcommerceApi.Models.Segment;

namespace EcommerceApi.Models.Rate;

public class Rate
{
    [Key]public int RateId { get; set; }
    public Guid ProductId { get; set; }
    [ForeignKey("ProductId")]
    public Product.Product Product { get; set; }
    public int UserId { get; set; }
    [ForeignKey("UserId")]
    public User User { get; set; }
    public string Content { get; set; } = string.Empty;
    public int Star { get; set; }
    public Boolean Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
}