using EcommerceApi.Dtos.User;
using EcommerceApi.ExtensionExceptions;
using EcommerceApi.Services.SsoService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace EcommerceApi.Authentication
{
    public class AuthenticationUserHandler : AuthenticationHandler<TokenAuthSchemeOptions>
    {
        private readonly ISsoService _ssoService;
        private readonly IHttpContextAccessor _httpContext;
        public AuthenticationUserHandler(IHttpContextAccessor httpContext, ISsoService ssoService,IOptionsMonitor<TokenAuthSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
            _ssoService = ssoService;
            _httpContext = httpContext;
        }
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var accessToken = _httpContext.HttpContext!.Request.Headers["Authorization"].ToString();
            if(!string.IsNullOrEmpty(accessToken))
            {
                accessToken = accessToken.Split(" ")[1];
            }

            var jsonString = await _ssoService.SsoTokenVerify(accessToken);

            SsoDto data = JsonSerializer.Deserialize<SsoDto>(jsonString)!;

            if(data.statusCode != 200)
            {
                throw new HttpStatusException((HttpStatusCode)data.statusCode, data.message);
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(data.token);
            // cần lấy về Role claim của User để Authorize
            var serviceName = jwtSecurityToken.Claims.First(claim => claim.Type == "serviceName").Value;
            var serviceUrl = jwtSecurityToken.Claims.First(claim => claim.Type == "serviceUrl").Value;
            var role = jwtSecurityToken.Claims.First(claim => claim.Type == "role").Value;

            var claims = new[] { 
                new Claim("ServiceName", serviceName),
                new Claim("ServiceUrl", serviceUrl),
                new Claim("Role", role),
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
