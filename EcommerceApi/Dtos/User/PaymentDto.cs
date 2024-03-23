namespace EcommerceApi.Dtos.User
{
    public class PaymentDto
    {

        public string PaymentContent { get; set; } = string.Empty;
        public string PaymentCurrency { get; set; } = string.Empty;
        public int RequiredAmount { get; set; }
        public string PaymentLanguage { get; set; } = string.Empty;
        //merchant
        public Guid MerchantId { get; set; }
        
        //destination
        public Guid DestinationId { get; set; }
        //order
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;


    }
}
