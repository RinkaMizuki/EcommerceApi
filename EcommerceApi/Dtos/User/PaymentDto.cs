namespace EcommerceApi.Dtos.User
{
    public class PaymentDto
    {

        public string PaymentContent { get; set; } = string.Empty;
        public string PaymentCurrency { get; set; } = string.Empty;
        public int RequiredAmount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime ExpiredAt { get; set; } = DateTime.Now.AddMinutes(15);
        public string PaymentLanguage { get; set; } = string.Empty;
        public string PaymentUrl { get; set; } = string.Empty;
        public string SignValue { get; set; } = string.Empty;
        public string SignAlgorithm { get; set; } = string.Empty;
        //merchant
        public string MerchantName { get; set; } = string.Empty;
        public string MerchantWebUrl { get; set; } = string.Empty;
        public string MerchantIpnUrl { get; set; } = string.Empty;
        public string MerchantReturnUrl { get; set; } = string.Empty;
        //destination
        public string DestinationName { get; set; } = string.Empty;

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
