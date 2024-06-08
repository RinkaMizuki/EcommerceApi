using EcommerceApi.Models.Provider;
using EcommerceApi.Models.Segment;
using EcommerceApi.Models.UserAddress;
using System.Text.Json.Serialization;

namespace EcommerceApi.Responses;

public class UserResponse
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "Member";
    public string Phone { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public Boolean EmailConfirm { get; set; }
    public Boolean IsActive { get; set; }
    public string Avatar { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public List<Segment> Segments { get; set; } = new List<Segment>();
    public List<UserLogins> UserLogins { get; set; } = new();
    public List<UserAddress> UserAddresses { get; set; } = new();
}