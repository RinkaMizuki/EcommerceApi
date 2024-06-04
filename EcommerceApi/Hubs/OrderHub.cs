using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EcommerceApi.Hubs
{
    [Authorize(Policy = "SsoAdmin")]
    public class OrderHub : Hub
    {
        
    }
}
