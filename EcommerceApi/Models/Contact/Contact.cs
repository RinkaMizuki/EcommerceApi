using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using EcommerceApi.Models.UserAddress;

namespace EcommerceApi.Models.Contact;

public class Contact
{
    [JsonPropertyName("id")]
    [Key] public int ContactId { get; set; }
    public Guid UserId { get; set; }
    [ForeignKey("UserId")] 
    [JsonIgnore]
    public User User { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime SentDate { get; set; }
}