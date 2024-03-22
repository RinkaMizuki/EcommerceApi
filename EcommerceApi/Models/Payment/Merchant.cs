using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.Models.Payment
{
    public class Merchant
    {
        [Key]
        public Guid MerchantId { get; set; }
        public string MerchantName { get; set; } = string.Empty;
        public string MerchantWebUrl { get; set; } = string.Empty;
        public string MerchantIpnUrl {  get; set; } = string.Empty;
        public string MerchantRetrunUrl { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public List<Payment> Payments { get; set; } = new();
    }
}
