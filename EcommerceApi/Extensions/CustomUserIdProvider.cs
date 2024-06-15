using Microsoft.AspNetCore.SignalR;

namespace EcommerceApi.Extensions
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            string adminId = connection.User.Claims.FirstOrDefault(claim => claim.Type == "UserId")!.Value;
            return adminId.ToString();
        }
    }
}
