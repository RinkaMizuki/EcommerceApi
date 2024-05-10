using Asp.Versioning;
using EcommerceApi.Dtos.Admin;
using EcommerceApi.Models.IdentityData;
using EcommerceApi.Services;
using EcommerceApi.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EcommerceApi.Dtos.User;
using System.Net;
using Azure.Core;
using EcommerceApi.ExtensionExceptions;
using System.Security.Claims;

namespace EcommerceApi.Controllers.V1.Admin
{
    //[Authorize(Policy = IdentityData.AdminPolicyRole)]
    [Authorize(Policy = "SsoAdmin")]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/Admin/")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("users/post")]
        public async Task<IActionResult> CreateUser(UserAdminDto userAdmin, CancellationToken userCancellationToken)
        {
            var userResponse = await _userService.PostUserAsync(userAdmin, userCancellationToken);
            return new JsonResult(new UserResponse()
                {
                    Id = userResponse.UserId,
                    UserName = userResponse.UserName,
                    Email = userResponse.Email,
                    Avatar = userResponse.Avatar,
                    BirthDate = Convert.ToDateTime(userResponse.BirthDate.ToShortDateString()),
                    EmailConfirm = userResponse.EmailConfirm,
                    IsActive = userResponse.IsActive,
                    Phone = userResponse.Phone,
                    Role = userResponse.Role.ToLower(),
                    Url = userResponse.Url,
                }
            );
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("users/{id:guid}")]
        public async Task<IActionResult> GetUser(Guid id, CancellationToken userCancellationToken)
        {
            var user = await _userService.GetUserByIdAsync(id, userCancellationToken);
            if (user == null) return NotFound();
            var userResponse = new UserResponse()
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
            };
            return new JsonResult(userResponse);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("users/{userName}")]
        public async Task<IActionResult> GetUserByUserName(string userName, CancellationToken userCancellationToken)
        {
            var isExisted = await _userService.GetUserByUserNameAync(userName, userCancellationToken);
            return StatusCode(200, new
            {
                isExisted,
                statusCode = 200
            });
        }

        [HttpPut]
        [Route("users/update/{id:guid}")]
        public async Task<IActionResult> UpdateUser([FromRoute] Guid id, [FromForm] UserAdminDto userAdmin,
            CancellationToken userCancellationToken)
        {
            var updatedUser = await _userService.UpdateUserByIdAsync(id, userAdmin, HttpContext, Request, userCancellationToken);

            var userResponse = new UserResponse()
            {
                Id = updatedUser.UserId,
                UserName = updatedUser.UserName,
                Email = updatedUser.Email,
                Avatar = updatedUser.Avatar,
                BirthDate = updatedUser.BirthDate,
                EmailConfirm = updatedUser.EmailConfirm,
                IsActive = updatedUser.IsActive,
                Phone = updatedUser.Phone,
                Role = updatedUser.Role.ToLower(),
                Url = updatedUser.Url,
            };
            return new JsonResult(userResponse);
        }

        [HttpDelete]
        [Route("users/delete/{id:guid}")]
        public async Task<IActionResult> DeleteUser(Guid id, CancellationToken userCancellationToken)
        {
            var response = await _userService.DeleteUserByIdAsync(id, userCancellationToken);
            if (!response) throw new Exception("User not found");
            return Ok(new
            {
                message = "Delete successfully"
            });
        }

        [HttpPut]
        [AllowAnonymous]
        [Route("users/update/profile/{id:guid}")]
        public async Task<IActionResult> UpdateProfile(Guid id,[FromForm]UserProfileDto userProfileDto, CancellationToken cancellationToken) { 
            var response = await _userService.UpdateUserProfile(id, Request, userProfileDto, cancellationToken);
            return StatusCode((int)HttpStatusCode.OK, new
            {
                user = response,
                statusCode = HttpStatusCode.OK,
            });
        }

        [HttpGet]
        [Route("users")]
        public async Task<IActionResult> GetListUsers(
            [FromQuery(Name = "sort")] string? sort,
            [FromQuery(Name = "range")] string? range,
            [FromQuery(Name = "filter")] string? filter,
            CancellationToken userCancellationToken
        )
        {
            var listUsers = await _userService.GetListUsersAsync(sort, range, filter, Response, userCancellationToken);
            return new JsonResult(listUsers);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("users/preview")]
        public async Task<IActionResult> GetUserAvatar(string avatar, CancellationToken userCancellationToken)
        {
            var userAvatar = await _userService.GetAvatarAsync(avatar, userCancellationToken);
            return File(userAvatar.FileStream, userAvatar.ContentType);
        }
        [HttpGet]
        [AllowAnonymous]
        [Route("users/confirm-email/{userId:guid}")]
        public async Task<IActionResult> UpdateConfirmEmail([FromRoute]Guid userId,
        CancellationToken cancellationToken)
        {
            await _userService.UpdateUserConfirm(userId, cancellationToken);
            return StatusCode(200, new { 
                message = "Update confirm successfully.",
                statusCode = 200
            });
        }
    }
}