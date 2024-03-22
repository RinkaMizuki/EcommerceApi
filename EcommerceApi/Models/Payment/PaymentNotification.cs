using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApi.Models.Payment
{
    public class PaymentNotification
    {
        [Key]
        public Guid NotificationId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string NotiAmount { get; set; } = string.Empty;
        public string NotiContent { get; set; } = string.Empty;
        public string NotiMessage { get; set; } = string.Empty;
        public Guid PaymentId { get; set; }
        [ForeignKey("PaymentId")]
        public Payment Payment { get; set; }
        public string NotiStatus { get; set; } = string.Empty;
        public DateTime NotiResDate { get; set; }
    }
}
