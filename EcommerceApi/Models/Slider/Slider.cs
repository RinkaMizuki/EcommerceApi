using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EcommerceApi.Models.Slider
{
    public class Slider
    {
        [Key]
        [JsonPropertyName("id")]
        public Guid SilderId { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ModifiedBy { get; set; } = string.Empty;
        public DateTime ModifiedAt { get; set; }
    }
}
