using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EcommerceApi.Models.Payment
{
    public class PaymentDestination
    {
        [Key]
        public Guid DestinationId { get; set; }
        public string DesLogo { get; set; } = string.Empty;
        public string DesShortName { get; set; } = string.Empty;
        public string DesName { get; set; } = string.Empty;
        public Guid? ParentDestinationId { get; set; }
        public bool IsActive { get; set; }
        [ForeignKey("ParentDestinationId")]
        [JsonIgnore]
        public PaymentDestination ParentPaymentDestination { get; set; } = null;
        public List<PaymentDestination> PaymentDestinationsChild { get; set; } = new();
    }
}
