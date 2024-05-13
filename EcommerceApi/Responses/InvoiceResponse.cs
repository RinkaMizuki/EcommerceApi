using EcommerceApi.Models.Order;
using EcommerceApi.Models.Payment;
using EcommerceApi.Models.UserAddress;

namespace EcommerceApi.Responses
{
    public class InvoiceResponse
    {
        public Guid Id { get; set; } = Guid.Empty;

        public Order Order { get; set; }
        public User User {  get; set; }
        public PaymentDestination PaymentDestination { get; set; }
        public string PaymentMessage { get; set; } = string.Empty;
        public string PaymentContent { get; set; } = string.Empty;
        public string PaymentCurrency { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int PaidAmout { get; set; }
    }
}
