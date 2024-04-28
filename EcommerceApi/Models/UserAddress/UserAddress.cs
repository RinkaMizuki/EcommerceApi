using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EcommerceApi.Models.UserAddress
{
    public class UserAddress
    {
        [Key]
        public Guid Id { get; set; }
        [ForeignKey("UserId")]
        [JsonIgnore]
        public User User { get; set; }
        public Guid UserId { get; set; }
        public string Name { get ; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Town { get; set; } = string.Empty;
        public bool IsDeliveryAddress { get; set; } = false;
        public bool IsPickupAddress { get; set; } = false;
        public bool IsReturnAddress { get; set; } = false;
    }
}
