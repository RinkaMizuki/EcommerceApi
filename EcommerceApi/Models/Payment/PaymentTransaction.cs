using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApi.Models.Payment
{
    public class PaymentTransaction
    {
        [Key]
        public Guid TranscationId { get; set; }
        public string TranMessage { get; set; } = string.Empty;
        public string TranPayload { get; set; } = string.Empty;
        public string TranStatus { get; set; } = string.Empty;
        public int TranAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid PaymentId { get; set; }
        [ForeignKey("PaymentId")]
        public Payment Payment { get; set; }
    }
}
