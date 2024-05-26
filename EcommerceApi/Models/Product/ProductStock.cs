using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EcommerceApi.Models.Product
{
    public class ProductStock
    {
        [JsonPropertyName("id")]
        [Key]
        public Guid ProductId { get; set; }
        [ForeignKey("ProductId")]
        [JsonIgnore]
        public Product Product { get; set; }
        public int StockQuantity { get; set; }
        public string Location { get; set; } = string.Empty;
    }
}
