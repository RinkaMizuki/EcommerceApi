using EcommerceApi.Dtos.Admin;
using EcommerceApi.Models.Segment;
using EcommerceApi.Responses;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApi.Services;

public interface IUserService
{
    public Task<User?> GetUserByIdAsync(int userId, CancellationToken userCancellationToken);
    public Task<List<UserResponse>> GetListUsersAsync(string sort,string range,string filter,HttpResponse Response, CancellationToken userCancellationToken);
    public Task<Boolean> DeleteUserByIdAsync(int userId, CancellationToken userCancellationToken);
    public Task<User> UpdateUserByIdAsync(int userId, UserAdminDto userAdminDto, HttpRequest request,CancellationToken userCancellationToken);
    public Task<User> PostUserAsync(UserAdminDto userAdmin, CancellationToken userCancellationToken);
    public Task<FileStreamResult> GetAvatarAsync(string avatarUrl, CancellationToken userCancellationToken);
}