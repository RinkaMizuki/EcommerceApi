using EcommerceApi.Dtos.User;
using EcommerceApi.ExtensionExceptions;
using EcommerceApi.Services.SsoService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace EcommerceApi.Authentication
{
    public class AuthenticationFacebookHandler : AuthenticationHandler<TokenAuthSchemeOptions>
    {
        private readonly ISsoService _ssoService;
        private readonly IHttpContextAccessor _httpContext;
        public AuthenticationFacebookHandler(IHttpContextAccessor httpContext, ISsoService ssoService, IOptionsMonitor<TokenAuthSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) 
            : base(options, logger, encoder, clock)
        {
            _ssoService = ssoService;
            _httpContext = httpContext;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var endpoint = _httpContext.HttpContext!.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
            {
                return AuthenticateResult.NoResult();
            }
            var accessToken = _httpContext.HttpContext!.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(accessToken))
            {
                accessToken = accessToken.Split(" ")[1];
            }
            var jsonString = await _ssoService.SsoFacebookTokenVerify(accessToken);

            SsoDto data = JsonSerializer.Deserialize<SsoDto>(jsonString)!;

            if (data.statusCode != 200)
            {
                return AuthenticateResult.Fail(new HttpStatusException((HttpStatusCode)data.statusCode, data.message));
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(data.token);
            // cần lấy về Role claim của User để Authorize
            var id = jwtSecurityToken.Claims.First(claim => claim.Type == "id").Value;
            var name = jwtSecurityToken.Claims.First(claim => claim.Type == "name").Value;
            var email = jwtSecurityToken.Claims.First(claim => claim.Type == "email").Value;

            var claims = new[] {
                new Claim("Id", id),
                new Claim("Name", name),
                new Claim("Email", email),
            };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Tokens"));
            var ticket = new AuthenticationTicket(principal, this.Scheme.Name);

            if (data.statusCode == 200)
            {
                return AuthenticateResult.Success(ticket);
            }
            return AuthenticateResult.Fail("Authentication failed.");
        }
    }
}
