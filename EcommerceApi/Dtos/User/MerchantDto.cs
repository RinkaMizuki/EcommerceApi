namespace EcommerceApi.Dtos.User
{
    public class MerchantDto
    {
        public string MerchantName { get; set; } = string.Empty;
        public string MerchantWebUrl { get; set; } = string.Empty;
        public string MerchantIpnUrl { get; set; } = string.Empty;
        public string MerchantRetrunUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
