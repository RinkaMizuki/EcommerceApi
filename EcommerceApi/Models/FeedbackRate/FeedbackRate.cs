using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EcommerceApi.Models.Feedback
{
    public class FeedbackRate
    {
        [ForeignKey("Rate")]
        public int FeedbackRateId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        [JsonIgnore]
        public Rate.Rate Rate { get; set; }
    }
}
