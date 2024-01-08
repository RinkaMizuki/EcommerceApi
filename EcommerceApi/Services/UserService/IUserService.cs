using EcommerceApi.Dtos.Admin;
using EcommerceApi.Models.Segment;
using EcommerceApi.Responses;

namespace EcommerceApi.Services;

public interface IUserService
{
    public Task<User?> GetUserByIdAsync(int userId);
    public Task<List<UserResponse>> GetListUsersAsync(string sort,string range,string filter,HttpResponse Response);
    public Task<Boolean> DeleteUserByIdAsync(int userId, CancellationToken userCancellationToken);
    public Task<User> UpdateUserByIdAsync(int userId, UserAdminDto userAdminDto, HttpRequest request,CancellationToken userCancellationToken);
    public Task<User> PostUserAsync(UserAdminDto userAdmin);
}