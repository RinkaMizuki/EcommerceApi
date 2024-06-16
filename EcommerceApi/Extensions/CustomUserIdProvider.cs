using Microsoft.AspNetCore.SignalR;

namespace EcommerceApi.Extensions
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            return connection.User.Claims.FirstOrDefault(claim => claim.Type == "Email")!.Value;
        }
    }
}
