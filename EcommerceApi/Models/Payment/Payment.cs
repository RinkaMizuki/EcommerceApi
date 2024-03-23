using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApi.Models.Payment
{
    public class Payment
    {
        [Key]
        public Guid PaymentId { get; set; }
        public string PaymentContent { get; set; } = string.Empty;
        public string PaymentCurrency { get; set; } = string.Empty;
        public Guid PaymentOrderId { get; set; }
        [ForeignKey("PaymentOrderId")]
        public Order.Order Order { get; set; }
        public int RequiredAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiredAt { get; set; }
        public string PaymentLanguage { get; set; } = string.Empty;
        public string PaymentUrl { get; set; } = string.Empty;
        public Guid MerchantId { get; set; }
        [ForeignKey("MerchantId")]
        public Merchant Merchant { get; set; }
        public Guid PaymentDestinationId { get; set; }
        [ForeignKey("PaymentDestinationId")]
        public PaymentDestination PaymentDestination { get; set; }
        public int PaidAmout { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public string PaymentLastMessage { get; set; } = string.Empty;
        public List<PaymentNotification> PaymentNotifications { get; set; } = new();
        public List<PaymentSignature> PaymentSignatures { get; set; } = new();
        public List<PaymentTransaction> PaymentTransactions { get; set; } = new();
    }
}
