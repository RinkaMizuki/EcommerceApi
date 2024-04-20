using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Asp.Versioning;
using EcommerceApi.Dtos.User;
using UserModel = EcommerceApi.Models.UserAddress.User;
using MessageModel = EcommerceApi.Models.Message.Message;
using EcommerceApi.Models;
using EcommerceApi.Responses;
using EcommerceApi.Services.ConfirmService;
using EcommerceApi.Services.MailService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using EcommerceApi.ExtensionExceptions;
using EcommerceApi.Models.Provider;
using System.Text.Json;

namespace EcommerceApi.Controllers.V1.User
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly EcommerceDbContext _context;
        private readonly IConfiguration _config;
        private readonly IMailService _mailService;
        private readonly IConfirmService _confirmService;

        public AuthController(EcommerceDbContext context, IConfiguration config, IMailService mailService,
            IConfirmService confirmService)
        {
            _context = context;
            _config = config;
            _mailService = mailService;
            _confirmService = confirmService;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(UserDto userDto, CancellationToken cancellationToken)
        {
            //if (string.IsNullOrEmpty(userDto.UserName)
            //    || string.IsNullOrEmpty(userDto.Email)
            //    || string.IsNullOrEmpty(userDto.Password)
            //    || string.IsNullOrEmpty(userDto.ConfirmPassword)
            //    || userDto.Password != userDto.ConfirmPassword
            //   )
            //{
            //    return BadRequest(new
            //    {
            //        message = "Register failed",
            //        statusCode = HttpStatusCode.BadRequest,
            //    });
            //}

            //var isAccountExist =
            //    await _context.Users.AnyAsync(u => u.UserName == userDto.UserName || u.Email == userDto.Email,
            //        cancellationToken);
            //if (isAccountExist)
            //{
            //    return new JsonResult(new
            //    {
            //        message =
            //            "The request could not be completed due to a conflict with the current state of the resource.",
            //        statusCode = HttpStatusCode.Conflict,
            //    });
            //}

            //var passwordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
            var newUser = new UserModel()
            {
                UserName = userDto.UserName,
                Email = userDto.Email,
                //PasswordHash = passwordHash,
                BirthDate = DateTime.Now,
                CreatedAt = DateTime.Now,
                ModifiedAt = DateTime.Now,
            };
            await _context.Users.AddAsync(newUser, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var token = _confirmService.GenerateEmailConfirmToken(newUser, 1);
            var domain = HttpContext.Request.Headers["origin"];

            var message =
                $"{domain}/confirm-email?email={newUser.Email}&token={token}";

            await _mailService.SendEmailAsync(new MessageModel(
                newUser.Email,
                newUser.UserName,
                "Please confirm your email.",
                $"Click here to confirm your email. <a href=\"{message}\">Click here!</a>"
            ), cancellationToken);

            return StatusCode(201, new
            {
                message = "Register successfully.",
                statusCode = HttpStatusCode.Created,
            });
        }

        [HttpGet]
        [Route("resend-confirm-email/{id:int}")]
        public async Task<IActionResult> ResendConfirmEmail(int id, CancellationToken cancellationToken)
        {
            var userReconfirm = await _context
                                              .Users
                                              .Where(u => u.UserId == id)
                                              .FirstOrDefaultAsync(cancellationToken)
                                              ?? throw new HttpStatusException(HttpStatusCode.NotFound, "User not found.");

            var token = _confirmService.GenerateEmailConfirmToken(userReconfirm, 1);

            var domain = HttpContext.Request.Headers["origin"];
            
            var message =
                $"{domain}/confirm-email?userId={userReconfirm.UserId}&token={token}";

            await _mailService.SendEmailAsync(new MessageModel(
                userReconfirm.Email,
                userReconfirm.UserName,
                "Please confirm your email.",
                $"Click here to confirm your email. <a href=\"{message}\">Click here!</a>"
            ), cancellationToken);
            return StatusCode(200, new
            {
                message = "Resend email confirm successfully. Please check your email !",
                statusCode = HttpStatusCode.OK,
            });
        }

        [HttpGet]
        [Route("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] int userId, [FromQuery] string token,
            CancellationToken cancellationToken)
        {
            var userConfirm = await _context
                                  .Users
                                  .Where(u => u.UserId == userId)
                                  .FirstOrDefaultAsync(cancellationToken)
                                   ?? throw new HttpStatusException(HttpStatusCode.NotFound, "User not found.");
            if (_confirmService.ValidateEmailConfirmationToken(token, out ClaimsPrincipal claimsPrincipal))
            {
                userConfirm.EmailConfirm = true;
                await _context.SaveChangesAsync(cancellationToken);
                return StatusCode(200, new
                {
                    message = "Confirm email successfully",
                    statusCode = 200,
                    user = new UserResponse()
                    {
                        Id = userConfirm.UserId,
                        UserName = userConfirm.UserName,
                        Email = userConfirm.Email,
                        Avatar = userConfirm.Avatar,
                        BirthDate = userConfirm.BirthDate,
                        EmailConfirm = userConfirm.EmailConfirm,
                        IsActive = userConfirm.IsActive,
                        Phone = userConfirm.Phone,
                        Role = userConfirm.Role.ToLower(),
                        Url = userConfirm.Url,
                    }
                });
            }
            else
            {
                return StatusCode(400, new
                {
                    message = "Confirm email failed",
                    statusCode = 400,
                });
            }
        }
        [HttpPost]
        [Route("google-auth")]
        public async Task<IActionResult> GoogleAuth([FromBody]ProviderDto providerDto, CancellationToken cancellationToken)
        {
            //case 1 : có tk rồi và đăng nhập google trùng với tk
            //case 2 : chưa có tk và đăng nhập bằng google
            try
            {
                var userLink = await _context
                                             .Users
                                             .Where(u => u.Email == providerDto.Email)
                                             .AsNoTracking()
                                             .FirstOrDefaultAsync(cancellationToken);

                var userLogins = await _context
                                               .UserLogins
                                               .Where(ul => ul.ProviderKey == providerDto.ProviderId)
                                               .FirstOrDefaultAsync(cancellationToken);

                if (userLogins != null)
                {
                    var user = await _context
                                                 .Users
                                                 .Include(u => u.UserLogins)
                                                 .Where(u => u.UserId == userLogins.UserId)
                                                 .FirstOrDefaultAsync(cancellationToken);
                    return Ok(new
                    {
                        statusCode = HttpStatusCode.OK,
                        user = new UserResponse()
                        {
                            Id = user.UserId,
                            UserName = user.UserName,
                            Email = user.Email,
                            Avatar = user.Avatar,
                            BirthDate = user.BirthDate,
                            EmailConfirm = user.EmailConfirm,
                            IsActive = user.IsActive,
                            Phone = user.Phone,
                            Role = user.Role.ToLower(),
                            Url = user.Url,
                            UserLogins = user.UserLogins
                        },
                    });
                }
                if (userLink != null && userLogins is null)
                {
                    var newProvider = new UserLogins()
                    {
                        UserId = userLink.UserId,
                        LoginProvider = providerDto.ProviderName,
                        ProviderDisplayName = providerDto.ProviderDisplayName,
                        ProviderKey = providerDto.ProviderId,
                        AccountAvatar = providerDto.Picture,
                        AccountName = providerDto.Email,
                        IsUnlink = false,
                    };
                    await _context.UserLogins.AddAsync(newProvider, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);

                    var userLinkSameAccount = await _context
                                                            .Users
                                                            .Include(u => u.UserLogins)
                                                            .Where(u => u.UserId == newProvider.UserId).FirstOrDefaultAsync
                                                            (cancellationToken);

                    return Ok(new
                    {
                        statusCode = HttpStatusCode.OK,
                        user = new UserResponse()
                        {
                            Id = userLink.UserId,
                            UserName = userLink.UserName,
                            Email = userLink.Email,
                            Avatar = userLink.Avatar,
                            BirthDate = userLink.BirthDate,
                            EmailConfirm = userLink.EmailConfirm,
                            IsActive = userLink.IsActive,
                            Phone = userLink.Phone,
                            Role = userLink.Role.ToLower(),
                            Url = userLink.Url,
                            UserLogins = userLinkSameAccount.UserLogins,
                        },
                    });
                }
                else
                {
                    var newUserWithProvider = new UserModel()
                    {
                        UserName = providerDto.Email,
                        Email = providerDto.Email,
                        BirthDate = DateTime.Now,
                        CreatedAt = DateTime.Now,
                        ModifiedAt = DateTime.Now,
                        EmailConfirm = true,
                        Url = providerDto.Picture,
                        Avatar = "Provider Avatar",
                    };
                    var newProvider = new UserLogins()
                    {
                        User = newUserWithProvider,
                        LoginProvider = providerDto.ProviderName,
                        ProviderDisplayName = providerDto.ProviderDisplayName,
                        ProviderKey = providerDto.ProviderId,
                        AccountAvatar = providerDto.Picture,
                        AccountName = providerDto.Email,
                        IsUnlink = false,
                    };
                    await _context.UserLogins.AddAsync(newProvider, cancellationToken);
                    await _context.Users.AddAsync(newUserWithProvider, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);

                    var newUser = await _context
                                                .Users
                                                .Include(u => u.UserLogins)
                                                .Where(u => u.Email == providerDto.Email)
                                                .FirstOrDefaultAsync(cancellationToken);
                    return Ok(new
                    {
                        statusCode = HttpStatusCode.OK,
                        user = new UserResponse()
                        {
                            Id = newUser.UserId,
                            UserName = newUser.UserName,
                            Email = newUser.Email,
                            Avatar = newUser.Avatar,
                            BirthDate = newUser.BirthDate,
                            EmailConfirm = newUser.EmailConfirm,
                            IsActive = newUser.IsActive,
                            Phone = newUser.Phone,
                            Role = newUser.Role.ToLower(),
                            Url = newUser.Url,
                            UserLogins = newUser.UserLogins,
                        },
                    });
                }
            }catch(Exception ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpPost]
        [Route("google-link")]
        public async Task<IActionResult> GoogleLink([FromQuery]int userId, [FromBody]ProviderDto providerDto, CancellationToken cancellationToken)
        {
            //case 3 : có tk rồi nhưng vào tk đó lk khác gmail
            //case 4 : có tk rồi đăng nhập bằng tk và lk cùng gmail
            var providerExternalLink = await _context
                                               .UserLogins
                                               .Where(ul => ul.ProviderKey == providerDto.ProviderId)
                                               .FirstOrDefaultAsync(cancellationToken);
            if(providerExternalLink == null)
            {
                var userLink = await _context
                                            .Users
                                            .Include(u => u.UserLogins)
                                            .Where(u => u.UserId == userId)
                                            .FirstOrDefaultAsync(cancellationToken)
                                            ?? throw new HttpStatusException(HttpStatusCode.NotFound, "User not found");
                var newProvider = new UserLogins()
                {
                    User = userLink,
                    LoginProvider = providerDto.ProviderName,
                    ProviderDisplayName = providerDto.ProviderDisplayName,
                    ProviderKey = providerDto.ProviderId,
                    AccountAvatar = providerDto.Picture,
                    AccountName = providerDto.Email,
                };
                await _context.UserLogins.AddAsync(newProvider, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                return Ok(new
                {
                    statusCode = HttpStatusCode.OK,
                    user = new UserResponse()
                    {
                        Id = userLink.UserId,
                        UserName = userLink.UserName,
                        Email = userLink.Email,
                        Avatar = userLink.Avatar,
                        BirthDate = userLink.BirthDate,
                        EmailConfirm = userLink.EmailConfirm,
                        IsActive = userLink.IsActive,
                        Phone = userLink.Phone,
                        Role = userLink.Role.ToLower(),
                        Url = userLink.Url,
                        UserLogins = userLink.UserLogins,
                    },
                });
            }
            else
            {
                throw new HttpStatusException(HttpStatusCode.Conflict, "Google account is invalid or already in use.");
            }
        }
        [HttpDelete]
        [Route("unlink-account")]
        public async Task<IActionResult> UnlinkAccount([FromQuery]int userId, [FromQuery] string providerId, CancellationToken cancellationToken)
        {
            var userUnlink = await _context
                                            .UserLogins
                                            .Where(ul => ul.UserId == userId && ul.ProviderKey == providerId)
                                            .FirstOrDefaultAsync(cancellationToken)
                                            ?? throw new HttpStatusException(HttpStatusCode.NotFound, "User or Provider not found.");
            _context.UserLogins.Remove(userUnlink);
            await _context.SaveChangesAsync(cancellationToken);
            return StatusCode(204, new
            {
                message = "Delete provider successfully",
                statusCode = 204,
            });
        }
        [HttpPost]
        [Route("facebook-auth")]
        public async Task<IActionResult> FacebookAuth([FromQuery]string type,[FromQuery]int? userId, [FromQuery]string facebookAccessToken, CancellationToken cancellationToken)
        {
            var userProfile  = await GetFacebookUserProfileAsync(facebookAccessToken);
            if(userProfile is null)
            {
                return BadRequest();
            }
            var userLogins = await _context
                                           .UserLogins
                                           .Where(ul => ul.ProviderKey == userProfile.Id)
                                           .FirstOrDefaultAsync(cancellationToken);
            if(userLogins != null)
            {
                if(type == "login")
                {
                    var user = await _context
                                             .Users
                                             .Include(u => u.UserLogins)
                                             .Where(u => u.UserId == userLogins.UserId)
                                             .FirstOrDefaultAsync(cancellationToken);
                    var accessToken = GenerateFacebookToken(userProfile);
                    var refreshToken = GenerateRefreshToken();
                    SetCookieRefreshToken(refreshToken);
                    refreshToken.User = user;
                    refreshToken.UserId = userLogins.UserId;
                    await _context
                                .RefreshTokens
                                .AddAsync(refreshToken, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);
                    return Ok(new
                    {
                        statusCode = HttpStatusCode.OK,
                        user = new UserResponse()
                        {
                            Id = user.UserId,
                            UserName = user.UserName,
                            Email = user.Email,
                            Avatar = user.Avatar,
                            BirthDate = user.BirthDate,
                            EmailConfirm = user.EmailConfirm,
                            IsActive = user.IsActive,
                            Phone = user.Phone,
                            Role = user.Role.ToLower(),
                            Url = user.Url,
                            UserLogins = user.UserLogins
                        },
                        accessToken,
                    });
                }
                //nếu type link
                //...
                else
                {
                    throw new HttpStatusException(HttpStatusCode.Conflict, "Facebook account is invalid or already in use.");
                }
            }
            else
            {
                //nếu type login
                if (type == "login")
                {

                    var isExistedEmail = await _context.Users.AnyAsync(u => u.Email == userProfile.Email,cancellationToken);
                    if(isExistedEmail)
                    {
                        throw new HttpStatusException(HttpStatusCode.Conflict, $"Email {userProfile.Email} is already used by a login method other than Facebook.");
                    }
                    var newUser = new UserModel()
                    {
                        UserName = userProfile.Email,
                        Email = userProfile.Email,
                        BirthDate = DateTime.Now,
                        CreatedAt = DateTime.Now,
                        ModifiedAt = DateTime.Now,
                        EmailConfirm = true,
                        Url = userProfile.Picture.Data.Url,
                        Avatar = "Provider Avatar",
                    };

                    var newProvider = new UserLogins()
                    {
                        User = newUser,
                        LoginProvider = "Facebook",
                        ProviderDisplayName = "Facebook",
                        ProviderKey = userProfile.Id,
                        AccountAvatar = userProfile.Picture.Data.Url,
                        AccountName = userProfile.Name,
                    };

                    var accessToken = GenerateFacebookToken(userProfile);
                    var refreshToken = GenerateRefreshToken();
                    SetCookieRefreshToken(refreshToken);
                    refreshToken.User = newUser;

                    await _context
                                  .UserLogins
                                  .AddAsync(newProvider, cancellationToken);
                    await _context
                                  .RefreshTokens
                                  .AddAsync(refreshToken, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);

                    var currentUser = await _context
                                                    .Users
                                                    .Include(u => u.UserLogins)
                                                    .Where(u => u.UserId == newUser.UserId)
                                                    .FirstOrDefaultAsync(cancellationToken);

                    return Ok(new
                    {
                        statusCode = HttpStatusCode.OK,
                        user = new UserResponse()
                        {
                            Id = currentUser.UserId,
                            UserName = newUser.UserName,
                            Email = newUser.Email,
                            Avatar = newUser.Avatar,
                            BirthDate = newUser.BirthDate,
                            EmailConfirm = newUser.EmailConfirm,
                            IsActive = newUser.IsActive,
                            Phone = newUser.Phone,
                            Role = newUser.Role.ToLower(),
                            Url = newUser.Url,
                            UserLogins = currentUser.UserLogins,
                        },
                        accessToken,
                    });
                }
                //nếu type link => lấy userId đang login
                //...
                else
                {
                    var userLink = await _context
                                                 .Users
                                                 .Include(u => u.UserLogins)
                                                 .Where(u => u.UserId == userId)
                                                 .FirstOrDefaultAsync(cancellationToken)
                                                 ?? throw new HttpStatusException(HttpStatusCode.NotFound, "User not found.");
                    var newProvider = new UserLogins()
                    {
                        User = userLink,
                        LoginProvider = "Facebook",
                        ProviderDisplayName = "Facebook",
                        ProviderKey = userProfile.Id,
                        AccountAvatar = userProfile.Picture.Data.Url,
                        AccountName = userProfile.Name,
                    };
                    var accessToken = GenerateFacebookToken(userProfile);
                    var refreshToken = GenerateRefreshToken();
                    SetCookieRefreshToken(refreshToken);
                    refreshToken.User = userLink;
                    refreshToken.UserId = userLink.UserId;

                    var removeToken = await _context
                                                    .RefreshTokens
                                                    .Where(rt => rt.UserId == userLink.UserId)
                                                    .FirstOrDefaultAsync(cancellationToken);
                    if(removeToken != null)
                    {
                        _context.RefreshTokens.Remove(removeToken);
                    }
                    await _context
                                .RefreshTokens
                                .AddAsync(refreshToken, cancellationToken);
                    await _context
                                  .UserLogins
                                  .AddAsync(newProvider, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);
                    return Ok(new
                    {
                        statusCode = HttpStatusCode.OK,
                        user = new UserResponse()
                        {
                            Id = userLink.UserId,
                            UserName = userLink.UserName,
                            Email = userLink.Email,
                            Avatar = userLink.Avatar,
                            BirthDate = userLink.BirthDate,
                            EmailConfirm = userLink.EmailConfirm,
                            IsActive = userLink.IsActive,
                            Phone = userLink.Phone,
                            Role = userLink.Role.ToLower(),
                            Url = userLink.Url,
                            UserLogins = userLink.UserLogins,
                        },
                        accessToken,
                    });
                }
            }

        }

        //[HttpPost]
        //[Route("login")]
        //public async Task<IActionResult> Login(LoginDto loginDto, CancellationToken cancellationToken)
        //{
        //    if (string.IsNullOrEmpty(loginDto.UserNameOrEmail)
        //        || string.IsNullOrEmpty(loginDto.Password))
        //    {
        //        return BadRequest(new
        //        {
        //            message = "Login failed.",
        //            statusCode = HttpStatusCode.BadRequest,
        //        });
        //    }
        //    //check confirm email and username, email, password
        //    var userLogin = await _context.Users
        //        .Where(u => (u.UserName == loginDto.UserNameOrEmail || u.Email == loginDto.UserNameOrEmail) && u.EmailConfirm)
        //        .FirstOrDefaultAsync(cancellationToken);
        //    if (userLogin == null)
        //    {
        //        return BadRequest(new
        //        {
        //            message = "Login failed.",
        //            statusCode = HttpStatusCode.BadRequest,
        //        });
        //    }

        //    var isMatchPassword = BCrypt.Net.BCrypt.Verify(loginDto.Password, userLogin.PasswordHash);
        //    if (!isMatchPassword ||
        //        userLogin.UserName != loginDto.UserNameOrEmail && userLogin.Email != loginDto.UserNameOrEmail)
        //    {
        //        return BadRequest(new
        //        {
        //            message = "Login failed.",
        //            statusCode = HttpStatusCode.BadRequest,
        //        });
        //    }

        //    var accessToken = GenerateAccessToken(GetListClaim(userLogin));
        //    var refreshToken = GenerateRefreshToken();
        //    SetCookieRefreshToken(refreshToken);
        //    refreshToken.User = userLogin;
        //    refreshToken.UserId = userLogin.UserId;

        //    await _context
        //                  .RefreshTokens
        //                  .AddAsync(refreshToken, cancellationToken);

        //    await _context.SaveChangesAsync(cancellationToken);
        //    return Ok(new
        //    {
        //        statusCode = HttpStatusCode.OK,
        //        user = new UserResponse()
        //        {
        //            Id = userLogin.UserId,
        //            UserName = userLogin.UserName,
        //            Email = userLogin.Email,
        //            Avatar = userLogin.Avatar,
        //            BirthDate = userLogin.BirthDate,
        //            EmailConfirm = userLogin.EmailConfirm,
        //            IsActive = userLogin.IsActive,
        //            Phone = userLogin.Phone,
        //            Role = userLogin.Role.ToLower(),
        //            Url = userLogin.Url,
        //        },
        //        accessToken,
        //    });
        //}

        //[HttpPost]
        //[Route("refresh-token")]
        //public async Task<IActionResult> RefreshToken(CancellationToken cancellationToken)
        //{
        //    var refreshToken = Request.Cookies["refreshToken"];
        //    if (string.IsNullOrEmpty(refreshToken))
        //    {
        //        return Forbid();
        //    }

        //    var currentRt = await _context.RefreshTokens.Where(rt => rt.Token == refreshToken)
        //        .FirstOrDefaultAsync(cancellationToken);
        //    if (currentRt == null)
        //    {
        //        return Unauthorized();
        //    }

        //    var user = await _context.Users.Where(u => u.UserId == currentRt.UserId).FirstOrDefaultAsync(cancellationToken);
        //    if (user == null) return Unauthorized();
        //    if (currentRt.Expires < DateTime.Now)
        //    {
        //        return Unauthorized();
        //    }

        //    var at = GenerateAccessToken(GetListClaim(user));
        //    var rt = GenerateRefreshToken();
        //    SetCookieRefreshToken(rt);

        //    currentRt.Expires = rt.Expires;
        //    currentRt.Token = rt.Token;
        //    currentRt.CreatedAt = rt.CreatedAt;

        //    await _context.SaveChangesAsync(cancellationToken);
        //    return Ok(new
        //    {
        //        statusCode = HttpStatusCode.OK,
        //        user = new
        //        {
        //            userName = user.UserName,
        //            email = user.Email,
        //            role = user.Role,
        //        },
        //        message = "refresh token successfully",
        //        accessToken = at,
        //    });
        //}

        //[HttpPost]
        //[Route("logout")]
        //public async Task<IActionResult> Logout(int userId)
        //{
        //    var refreshToken = Request.Cookies["refreshToken"];
        //    var token = await _context.RefreshTokens.Where(rt => rt.UserId == userId || refreshToken == rt.Token)
        //        .FirstOrDefaultAsync();
        //    if (token == null) return NotFound();
        //    _context.RefreshTokens.Remove(token);
        //    await _context.SaveChangesAsync();
        //    Response.Cookies.Delete("refreshToken");
        //    return Ok(new
        //    {
        //        message = "Logout successfully.",
        //        statusCode = HttpStatusCode.OK
        //    });
        //}

        //private List<Claim> GetListClaim(UserModel user)
        //{
        //    var claims = new List<Claim>
        //    {
        //        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        //        new(JwtRegisteredClaimNames.Email, user.Email, ClaimTypes.Email),
        //        new(JwtRegisteredClaimNames.UniqueName, user.UserName, ClaimTypes.Name),
        //        new("UserId", user.UserId.ToString()),
        //        new("Role", user.Role),
        //    };
        //    return claims;
        //}
        private async Task<FacebookUser?> GetFacebookUserProfileAsync(string accessToken)
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"https://graph.facebook.com/v19.0/me?fields=id,name,email,picture&access_token={accessToken}");

            if (response.IsSuccessStatusCode)
            {
                var userJson = await response.Content.ReadAsStringAsync();
                var facebookUser = JsonSerializer.Deserialize<FacebookUser>(userJson);
                return facebookUser;
            }

            return null;
        }
        private string GenerateFacebookToken(FacebookUser facebookUser)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, facebookUser.Id),
                new Claim(JwtRegisteredClaimNames.Name, facebookUser.Name),
                new Claim(JwtRegisteredClaimNames.Email, facebookUser.Email),
                new Claim("Picture", JsonSerializer.Serialize(facebookUser.Picture)),
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("FacebookConfiguration:AppSecret").Value ?? ""));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config.GetSection("FacebookConfiguration:FacebookIssuer").Value,
                audience: _config.GetSection("FacebookConfiguration:AppId").Value,
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateAccessToken(List<Claim> claims)
        {
            var securityKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("JwtConfiguration:Secret").Value ?? ""));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _config.GetSection("JwtConfiguration:ValidIssuer").Value,
                audience: _config.GetSection("JwtConfiguration:ValidAudience").Value,
                expires: DateTime.Now.AddMinutes(30),
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
                Expires = DateTime.Now.AddDays(7),
                CreatedAt = DateTime.Now
            };
            return refreshToken;
        }

        private void SetCookieRefreshToken(RefreshToken refreshToken)
        {
            var cookieOptions = new CookieOptions()
            {
                HttpOnly = true,
                Secure = false,
                Expires = DateTime.Now.AddDays(7),
            };
            Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);
        }
    }
}