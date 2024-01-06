using EcommerceApi.Models.Coupon;
using EcommerceApi.Models.Order;
using EcommerceApi.Models.Rate;

namespace EcommerceApi.Responses;

public class UserResponse
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "Member";
    public string Phone { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public Boolean EmailConfirm { get; set; }
    public Boolean IsActive { get; set; }
    public string Avatar { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}