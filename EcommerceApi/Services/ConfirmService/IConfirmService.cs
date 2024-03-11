using EcommerceApi.Models.UserAddress;
using System.Security.Claims;

namespace EcommerceApi.Services.ConfirmService
{
    public interface IConfirmService
    {
        public string GenerateEmailConfirmToken(User user, int expirationHours = 24);
        public bool ValidateEmailConfirmationToken(string token, out ClaimsPrincipal claimsPrincipal);
    }
}
