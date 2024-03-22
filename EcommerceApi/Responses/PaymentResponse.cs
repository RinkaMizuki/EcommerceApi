namespace EcommerceApi.Responses
{
    public class PaymentResponse
    {
        public string PaymentUrl { get; set; } = string.Empty;
        public Guid PaymentId { get; set; }
    }
}
