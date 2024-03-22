using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApi.Models.Payment
{
    public class PaymentSignature
    {
        [Key]
        public Guid SignatureId { get; set; }
        public string SignValue { get; set; } = string.Empty;
        public string SignAlgorithm { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string SignOwn { get; set; } = string.Empty;
        public Payment Payment { get; set; }
        [ForeignKey("PaymentId")]
        public Guid PaymentId { get; set; }
    }
}
