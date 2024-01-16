using System.Net;
using Asp.Versioning;
using EcommerceApi.Dtos.Admin;
using EcommerceApi.Models;
using EcommerceApi.Models.IdentityData;
using EcommerceApi.Services;
using EcommerceApi.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommerceApi.Controllers.V1.Admin
{
    [Authorize(Policy = IdentityData.AdminPolicyRole)]
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
        [Route("users/{id:int}")]
        public async Task<IActionResult> GetUser(int id, CancellationToken userCancellationToken)
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

        [HttpPut]
        [Route("users/update/{id:int}")]
        public async Task<IActionResult> UpdateUser([FromRoute] int id, [FromForm] UserAdminDto userAdmin, CancellationToken userCancellationToken)
        {
            var updatedUser = await _userService.UpdateUserByIdAsync(id, userAdmin, Request, userCancellationToken);
            
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
        [Route("users/delete/{id:int}")]
        public async Task<IActionResult> DeleteUser(int id, CancellationToken userCancellationToken)
        {
            var response = await _userService.DeleteUserByIdAsync(id, userCancellationToken);
                if (!response) throw new Exception("User not found");
                return Ok(new
                {
                    message = "Delete successfully"
                });
        }

        [HttpGet]
        [Route("users")]
        public async Task<IActionResult> GetListUsers(
            [FromQuery(Name = "sort")] string sort,
            [FromQuery(Name = "range")] string range,
            [FromQuery(Name = "filter")] string filter,
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
    }
}