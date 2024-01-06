using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Asp.Versioning;
using EcommerceApi.Dtos.User;
using EcommerceApi.Models;
using EcommerceApi.Models.Segment;
using EcommerceApi.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace EcommerceApi.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly EcommerceDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(EcommerceDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserDto User)
        {
            if (string.IsNullOrEmpty(User.UserName)
                || string.IsNullOrEmpty(User.Email)
                || string.IsNullOrEmpty(User.Password)
                || string.IsNullOrEmpty(User.ConfirmPassword)
                || User.Password != User.ConfirmPassword
               )
            {
                return BadRequest(new
                {
                    message = "register failed",
                    statusCode = HttpStatusCode.BadRequest,
                });
            }

            var isAccountExist = _context.Users.Where(u => u.UserName == User.UserName || u.Email == User.Email).Any();
            if (isAccountExist)
            {
                return new JsonResult(new
                {
                    message =
                        "The request could not be completed due to a conflict with the current state of the resource.",
                    statusCode = HttpStatusCode.Conflict,
                });
            }

            var PasswordHash = BCrypt.Net.BCrypt.HashPassword(User.Password);
            var newUser = new User()
            {
                UserName = User.UserName,
                Email = User.Email,
                PasswordHash = PasswordHash,
                BirthDate = DateTime.Now,
                CreatedAt = DateTime.Now,
                ModifiedAt = DateTime.Now,
            };
            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();
            return Ok(new
            {
                message = "register successfully",
                statusCode = HttpStatusCode.Created,
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto Login)
        {
            if (string.IsNullOrEmpty(Login.UserNameOrEmail)
                || string.IsNullOrEmpty(Login.Password))
            {
                return BadRequest(new
                {
                    message = "login failed",
                    statusCode = HttpStatusCode.BadRequest,
                });
            }

            var userLogin = await _context.Users
                .Where(u => u.UserName == Login.UserNameOrEmail || u.Email == Login.UserNameOrEmail)
                .FirstOrDefaultAsync();
            if (userLogin == null)
            {
                return BadRequest(new
                {
                    message = "login failed",
                    statusCode = HttpStatusCode.BadRequest,
                });
            }

            var isMatchPassword = BCrypt.Net.BCrypt.Verify(Login.Password, userLogin.PasswordHash);
            if (!isMatchPassword ||
                (userLogin.UserName != Login.UserNameOrEmail && userLogin.Email != Login.UserNameOrEmail))
            {
                return BadRequest(new
                {
                    message = "login failed",
                    statusCode = HttpStatusCode.BadRequest,
                });
            }

            var accessToken = GenerateAccessToken(GetListClaim(userLogin));
            var refreshToken = GenerateRefreshToken();
            SetCookieRefreshToken(refreshToken);
            refreshToken.User = userLogin;
            refreshToken.UserId = userLogin.UserId;
            await _context.RefreshTokens.AddAsync(refreshToken);

            await _context.SaveChangesAsync();
            return Ok(new
            {
                message = "login successfully",
                statusCode = HttpStatusCode.OK,
                user = new UserResponse()
                {
                    Id = userLogin.UserId,
                    UserName = userLogin.UserName,
                    Email = userLogin.Email,
                    Avatar = userLogin.Avatar,
                    BirthDate = userLogin.BirthDate,
                    EmailConfirm = userLogin.EmailConfirm,
                    IsActive = userLogin.IsActive,
                    Phone = userLogin.Phone,
                    Role = userLogin.Role.ToLower(),
                    Url = userLogin.Url,
                },
                accessToken,
            });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized();
            }

            var currentRt = await _context.RefreshTokens.Where(rt => rt.Token == refreshToken)
                .FirstOrDefaultAsync();
            if (currentRt == null)
            {
                return Unauthorized();
            }

            var user = await _context.Users.Where(u => u.UserId == currentRt.UserId).FirstOrDefaultAsync();
            if (user == null) return Unauthorized();
            if (currentRt.Expries < DateTime.Now)
            {
                return Unauthorized();
            }

            var at = GenerateAccessToken(GetListClaim(user));
            var rt = GenerateRefreshToken();
            SetCookieRefreshToken(rt);
            currentRt.Expries = rt.Expries;
            currentRt.Token = rt.Token;
            currentRt.CreatedAt = rt.CreatedAt;

            await _context.SaveChangesAsync();
            return Ok(new
            {
                statusCode = HttpStatusCode.OK,
                user = new
                {
                    userName = user.UserName,
                    email = user.Email,
                    role = user.Role,
                },
                message = "refresh token successfully",
                accessToken = at,
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(int userId)
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var token = await _context.RefreshTokens.Where(rt => rt.UserId == userId || refreshToken == rt.Token)
                .FirstOrDefaultAsync();
            if (token == null) return BadRequest();
            _context.RefreshTokens.Remove(token);
            await _context.SaveChangesAsync();
            Response.Cookies.Delete("refreshToken");
            return Ok(new
            {
                message = "logout successfully",
                statusCode = HttpStatusCode.OK
            });
        }

        private List<Claim> GetListClaim(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email.ToString(), ClaimTypes.Email),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName, ClaimTypes.Name),
                new Claim("UserId", user.UserId.ToString()),
                new Claim("Role", user.Role),
            };
            return claims;
        }

        private string GenerateAccessToken(List<Claim> claims)
        {
            var securityKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("SecretKeyToken").Value ?? ""));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: "http://localhost:5083",
                audience: "http://localhost:5083",
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: credentials,
                claims: claims
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        private RefreshToken GenerateRefreshToken()
        {
            var refreshToken = new RefreshToken()
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expries = DateTime.Now.AddDays(7),
                CreatedAt = DateTime.Now
            };
            return refreshToken;
        }

        private void SetCookieRefreshToken(RefreshToken refreshToken)
        {
            var cookieOptions = new CookieOptions()
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                Secure = false,
                Expires = DateTime.Now.AddDays(7),
            };
            Console.WriteLine(refreshToken.Token);
            Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);
        }
    }
}