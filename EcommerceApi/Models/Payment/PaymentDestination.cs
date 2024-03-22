using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApi.Models.Payment
{
    public class PaymentDestination
    {
        [Key]
        public Guid DestinationId { get; set; }
        public string DesLogo { get; set; } = string.Empty;
        public string DesShortName { get; set; } = string.Empty;
        public string DesName { get; set; } = string.Empty;
        public int DesSortIndex { get; set;}
        public Guid? ParentDestinationId { get; set; }

        [ForeignKey("ParentDestinationId")]
        public PaymentDestination ParentPaymentDestination { get; set; } = null;
        public List<PaymentDestination> PaymentDestinationsChild { get; set; } = new();
        public bool IsActive { get; set; }
        public List<Payment> Payments { get; set; } = new();
        
    }
}
