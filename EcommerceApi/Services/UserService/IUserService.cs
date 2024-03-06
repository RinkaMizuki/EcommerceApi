using EcommerceApi.Dtos.Admin;
using EcommerceApi.Dtos.User;
using EcommerceApi.Models.Segment;
using EcommerceApi.Responses;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApi.Services;

public interface IUserService
{
    public Task<User?> GetUserByIdAsync(int userId, CancellationToken userCancellationToken);
    public Task<bool> GetUserByUserNameAync(string userName, CancellationToken userCancellationToken);
    public Task<List<UserResponse>> GetListUsersAsync(string sort,string range,string filter,HttpResponse response, CancellationToken userCancellationToken);
    public Task<bool> DeleteUserByIdAsync(int userId, CancellationToken userCancellationToken);
    public Task<UserResponse> UpdateUserProfile(int userId, HttpRequest request,UserProfileDto userProfileDto, CancellationToken userCancellationToken);
    public Task<User> UpdateUserByIdAsync(int userId, UserAdminDto userAdminDto, HttpContext httpContext, HttpRequest request,CancellationToken userCancellationToken);
    public Task<User> PostUserAsync(UserAdminDto userAdmin, CancellationToken userCancellationToken);
    public Task<FileStreamResult> GetAvatarAsync(string avatarUrl, CancellationToken userCancellationToken);
}