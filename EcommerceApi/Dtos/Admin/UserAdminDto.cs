namespace EcommerceApi.Dtos.Admin;

public class UserAdminDto
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "Member";
    public string? Phone { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public IFormFile? file { get; set; }
    public bool EmailConfirm { get; set; }
    public bool IsActive { get; set; }
    public string? Avatar { get; set; } = string.Empty;
}