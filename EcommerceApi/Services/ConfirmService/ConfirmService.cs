using EcommerceApi.ExtensionExceptions;
using EcommerceApi.Models.Segment;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace EcommerceApi.Services.ConfirmService
{
    public class ConfirmService : IConfirmService
    {
        private readonly IConfiguration _config;
        public ConfirmService(IConfiguration config) {
            _config = config;
        }
        public string GenerateEmailConfirmToken(User user, int expirationHours = 24)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_config.GetSection("SecretKeyConfirm").Value!);
                var securityKey = new SymmetricSecurityKey(key);
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: "http://localhost:5083",
                    audience: "http://localhost:5083",
                    expires: DateTime.Now.AddHours(expirationHours),
                    signingCredentials: credentials,
                    claims: new[] {
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(ClaimTypes.Email, user.Email),
                    }
                );
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public bool ValidateEmailConfirmationToken(string token, out ClaimsPrincipal claimsPrincipal)
        {
            claimsPrincipal = null;
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config.GetSection("SecretKeyConfirm").Value!);
            try
            {
                claimsPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = "http://localhost:5083",
                    ValidIssuer = "http://localhost:5083",
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                }, out SecurityToken validatedToken);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
