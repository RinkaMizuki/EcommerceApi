using EcommerceApi.Models.UserAddress;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EcommerceApi.Models.Provider
{
    public class UserLogins
    {
        public string LoginProvider { get; set; } = string.Empty;
        public string ProviderKey { get; set; } = string.Empty;
        public string ProviderDisplayName { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string AccountAvatar { get; set; } = string.Empty;
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        [JsonIgnore]
        public User User { get; set; }
        public bool IsUnlink { get; set; } = true;
    }
}
