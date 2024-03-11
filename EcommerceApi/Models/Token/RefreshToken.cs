using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcommerceApi.Models.UserAddress;

namespace EcommerceApi.Models;

public class RefreshToken
{
    [Key] public int TokenId { get; set; }
    [DataType(DataType.Text)] public string Token { get; set; } = string.Empty;
    public int UserId { get; set; }
    [ForeignKey("UserId")] public User User { get; set; }
    [DataType(DataType.Date)] public DateTime CreatedAt { get; set; }
    [DataType(DataType.Date)] public DateTime Expries { get; set; }
}