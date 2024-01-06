using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.Models.Segment;

public class User
{
    [Key] public int UserId { get; set; }
    [DataType(DataType.Text)] public string UserName { get; set; } = string.Empty;
    [DataType(DataType.EmailAddress)] public string Email { get; set; } = string.Empty;
    [DataType(DataType.Password)] public string PasswordHash { get; set; } = string.Empty;
    [DataType(DataType.Text)] public string Role { get; set; } = "member";
    public string Phone { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; } = DateTime.Now;
    public bool EmailConfirm { get; set; } = false;
    public bool IsActive { get; set; } = false;
    public string Avatar { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public List<Rate.Rate> Rates { get; set; }
    public List<Order.Order> Orders { get; set; }
    public List<UserSegment> UserSegments { get; set; }
}