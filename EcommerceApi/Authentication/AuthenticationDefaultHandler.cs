using EcommerceApi.Dtos.User;
using EcommerceApi.ExtensionExceptions;
using EcommerceApi.Services.RedisService;
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
    public class AuthenticationDefaultHandler : AuthenticationHandler<TokenAuthSchemeOptions>
    {
        private readonly ISsoService _ssoService;
        private readonly IRedisService _redisService;
        private readonly IHttpContextAccessor _httpContext;

        public AuthenticationDefaultHandler(IHttpContextAccessor httpContext, ISsoService ssoService,
            IRedisService redisService, IOptionsMonitor<TokenAuthSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
            _ssoService = ssoService;
            _redisService = redisService;
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
            var tokenDirty = "";
            if (string.IsNullOrEmpty(accessToken))
            {
                accessToken = Context.Request.Query["access_token"];
            }
            if(!string.IsNullOrEmpty(accessToken))
            {
                var tokenSplit = accessToken.Split(" ");
                if(tokenSplit.Length > 1)
                {
                    accessToken = tokenSplit[1];
                }
                tokenDirty = await _redisService.GetValueAsync(accessToken);
            }

            if(!string.IsNullOrEmpty(tokenDirty))
            {
                return AuthenticateResult.Fail(new HttpStatusException(HttpStatusCode.Forbidden, "Authentication failed. Because this token has been revoked."));
            }

            var jsonString = await _ssoService.SsoDefaultTokenVerify(accessToken);

            SsoDto data = JsonSerializer.Deserialize<SsoDto>(jsonString)!;

            if (data.statusCode != 200)
            {
                return AuthenticateResult.Fail(new HttpStatusException((HttpStatusCode)data.statusCode, data.message));
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(data.token);
            // cần lấy về Role claim của User để Authorize
            var serviceName = jwtSecurityToken.Claims.First(claim => claim.Type == "serviceName").Value;
            var serviceUrl = jwtSecurityToken.Claims.First(claim => claim.Type == "serviceUrl").Value;
            var role = jwtSecurityToken.Claims.First(claim => claim.Type == "role").Value;
            var email = jwtSecurityToken.Claims.First(claim => claim.Type == "email").Value;
            var userId = jwtSecurityToken.Claims.First(claim => claim.Type == "userId").Value;

            var claims = new[] { 
                new Claim("ServiceName", serviceName),
                new Claim("ServiceUrl", serviceUrl),
                new Claim("Role", role),
                new Claim("Email", email),
                new Claim("UserId", userId),
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
