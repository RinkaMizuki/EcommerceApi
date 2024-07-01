using EcommerceApi.Services.SsoService;
using Microsoft.AspNetCore.Authorization;

namespace EcommerceApi.Attributes
{
    public class AuthorizationPermissionHandler : IAuthorizationHandler
    {
        private readonly IHttpContextAccessor _httpContext;
        public AuthorizationPermissionHandler(IHttpContextAccessor httpContext)
        {
            _httpContext = httpContext;
        }
        private bool IsAdmin(IAuthorizationRequirement requirement)
        {
            var claimsPrincipal = _httpContext.HttpContext!.User;
            var require = requirement as AdminAccessApiRequirement;
            var correctRole = claimsPrincipal.HasClaim(c =>
                c.Type == "Role" && c.Value == require!.Role
            );
            var correctServiceName = claimsPrincipal.HasClaim(c =>
                c.Type == "ServiceName" && c.Value == require!.ServiceName
            );
            var correctServiceUrl = claimsPrincipal.HasClaim(c =>
             c.Type == "ServiceUrl" && c.Value == require!.ServiceUrl
            );
            if (correctRole && correctServiceName && correctServiceUrl)
            {
                return true;
            }
            return false;
        }

        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            var pendingRequirements = context.PendingRequirements.ToList();
            foreach (var requirement in pendingRequirements)
            {

                // Xử lý nếu requirement là GenZrequirement
                if (requirement is AdminAccessApiRequirement)
                {
                    if (IsAdmin(requirement))
                    {
                        context.Succeed(requirement);
                    }
                    else
                    {
                        context.Fail();
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}
