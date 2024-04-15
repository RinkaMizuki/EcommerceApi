using Microsoft.AspNetCore.Authorization;

namespace EcommerceApi.Attributes
{
    public class AdminAccessApiRequirement : IAuthorizationRequirement
    {
        public string ServiceName { get; } = string.Empty;
        public string ServiceUrl { get; } = string.Empty;
        public string Role { get; } = string.Empty;
        public AdminAccessApiRequirement(IConfiguration config)
        {
            ServiceName = config.GetSection("ServiceName").Value!;
            ServiceUrl = config.GetSection("ServiceUrl").Value!;
            Role = "admin";
        }
    }
}
