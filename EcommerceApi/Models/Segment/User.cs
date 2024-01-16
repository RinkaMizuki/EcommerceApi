using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EcommerceApi.Models.Segment;

public class User
{
    [Key] public int UserId { get; set; }
    [DataType(DataType.Text)] public string UserName { get; set; } = string.Empty;
    [DataType(DataType.EmailAddress)] public string Email { get; set; } = string.Empty;
    [JsonIgnore]
    [DataType(DataType.Password)] public string PasswordHash { get; set; } = string.Empty;
    [DataType(DataType.Text)] public string Role { get; set; } = "member";
    public string Phone { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; } = DateTime.Now;
    public bool EmailConfirm { get; set; } = false;
    public bool IsActive { get; set; } = false;
    public bool IsGetOff { get; set; } = false;
    public string Avatar { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    [JsonIgnore]
    public List<Rate.Rate> Rates { get; set; } = new List<Rate.Rate> ();
    public List<Order.Order> Orders { get; set; } = new List<Order.Order> ();
    public List<UserSegment> UserSegments { get; set; } = new List<UserSegment> ();
}