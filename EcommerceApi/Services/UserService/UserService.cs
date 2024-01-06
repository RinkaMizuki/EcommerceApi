using System.Net;
using EcommerceApi.Dtos.Admin;
using EcommerceApi.Dtos.Upload;
using EcommerceApi.Models;
using EcommerceApi.Models.Segment;
using EcommerceApi.Responses;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApi.Services;

public class UserService : IUserService
{
    private readonly EcommerceDbContext _context;
    private readonly ICloudflareClient _cloudflareClient;

    public UserService(EcommerceDbContext context, ICloudflareClient cloudflareClient)
    {
        _context = context;
        _cloudflareClient = cloudflareClient;
    }

    public async Task<User> PostUserAsync(UserAdminDto userAdmin)
    {
        if (_context.Users.AsNoTracking().Where(u => u.UserName == userAdmin.UserName || u.Email == userAdmin.Email)
            .Any())
        {
            return null;
        }

        var newUser = new User()
        {
            UserName = userAdmin.UserName,
            Email = userAdmin.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(userAdmin.Password),
            CreatedAt = DateTime.Now,
            ModifiedAt = DateTime.Now,
            Role = userAdmin.Role,
            Phone = userAdmin?.Phone,
            BirthDate = userAdmin.BirthDate,
            IsActive = userAdmin.IsActive,
            EmailConfirm = userAdmin.EmailConfirm,
        };
        await _context.Users.AddAsync(newUser);
        await _context.SaveChangesAsync();
        var userResponse = await _context.Users.AsNoTracking().Where(u => u.UserName == newUser.UserName)
            .FirstOrDefaultAsync();
        return userResponse;
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        return user;
    }

    public async Task<List<UserResponse>> GetListUsersAsync(string sort, string range, string filter,
        HttpResponse Response)
    {
        var listUser = await _context.Users.AsNoTracking().ToListAsync();
        List<int> rangeValues = Helpers.ParseString<int>(range);
        List<string> sortValues = Helpers.ParseString<string>(sort);
        List<string> filterValues = Helpers.ParseString<string>(filter);
        if (!filterValues.Contains("q"))
        {
            filterValues.Insert(0, "q");
            filterValues.Insert(1, "");
        }

        if (!filterValues.Contains("email"))
        {
            filterValues.Add("email");
            filterValues.Add("");
        }

        var perPage = rangeValues[1] - rangeValues[0] + 1;
        var currentPage = Convert.ToInt32(Math.Ceiling((double)rangeValues[0] / perPage)) + 1;
        var sortBy = sortValues[0].ToLower();
        var sortType = sortValues[1].ToLower();
        var listUsers = await _context.Users.Select(u => new UserResponse()
        {
            Id = u.UserId,
            UserName = u.UserName,
            Email = u.Email,
            Avatar = u.Avatar,
            BirthDate = Convert.ToDateTime(u.BirthDate.ToShortDateString()),
            EmailConfirm = u.EmailConfirm,
            IsActive = u.IsActive,
            Phone = u.Phone,
            Role = u.Role,
            Url = u.Url,
        }).ToListAsync();
        var totalUser = listUsers.Count;
        listUsers = listUsers
            .Where(u => (filterValues[3] != "" && filterValues[1] != ""
                    ? u.UserName.ToLower().Contains(filterValues[1].ToLower()) &&
                      u.Email.ToLower().Contains(filterValues[3].ToLower())
                    : filterValues[3] == "" && filterValues[1] == ""
                        ? filterValues[3] == "" && filterValues[1] == ""
                        : filterValues[1] != "" && filterValues[3] == ""
                            ? u.UserName.ToLower().Contains(filterValues[1].ToLower())
                            : u.Email.ToLower().Contains(filterValues[3].ToLower())
                ))
            .Skip((currentPage - 1) * perPage).Take(perPage).ToList();
        if (sortType == "desc") listUsers.Reverse();
        switch (sortType)
        {
            case "asc":
                switch (sortBy)
                {
                    case SortOrder.SortOrder.SortById:
                        listUsers = listUsers.OrderBy(u => u.Id).ToList();
                        break;
                    case SortOrder.SortOrder.SortByBirthDate:
                        listUsers = listUsers.OrderBy(u => u.BirthDate).ToList();
                        break;
                    default:
                        listUsers = listUsers.OrderBy(u => u.UserName).ToList();
                        break;
                }

                break;
            case "desc":
                switch (sortBy)
                {
                    case SortOrder.SortOrder.SortById:
                        listUsers = listUsers.OrderByDescending(u => u.Id).ToList();
                        break;
                    case SortOrder.SortOrder.SortByBirthDate:
                        listUsers = listUsers.OrderByDescending(u => u.BirthDate).ToList();
                        break;
                    default:
                        listUsers = listUsers.OrderByDescending(u => u.UserName).ToList();
                        break;
                }

                break;
        }

        Response.Headers.Append("Access-Control-Expose-Headers", "Content-Range");
        Response.Headers.Append("Content-Range", $"users {rangeValues[0]}-{rangeValues[1]}/{totalUser}");
        return listUsers;
    }



    public async Task<Boolean> DeleteUserByIdAsync(int userId)
    {
        var deleteUser = await this.GetUserByIdAsync(userId);
        if (deleteUser == null) return false;
        _context.Users.Remove(deleteUser);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<User> UpdateUserByIdAsync(int userId, UserAdminDto userAdminDto, HttpRequest request)
    {
        var updateUser = await this.GetUserByIdAsync(userId);
        if (updateUser == null) return null;
        if (_context.Users
            .Where(u => (u.UserName == userAdminDto.UserName || u.Email == userAdminDto.Email) && u.UserId != userId)
            .Any())
        {
            return null;
        }

        if (userAdminDto.file != null && userAdminDto.Avatar != "")
        {
            if (string.IsNullOrEmpty(updateUser.Url))
            {
                await _cloudflareClient.UploadImageAsync(new UploadDto()
                {
                    Id = updateUser.UserId,
                    File = userAdminDto.file,
                },"avatar");
            }
            else
            {
                var res = await _cloudflareClient.DeleteObjectAsync($"avatar_{updateUser.UserId}_{updateUser.Avatar}");
                if (res.HttpStatusCode == HttpStatusCode.NoContent)
                {
                    await _cloudflareClient.UploadImageAsync(new UploadDto()
                    {
                        Id = updateUser.UserId,
                        File = userAdminDto.file,
                    }, "avatar");
                }
            }

            updateUser.Url =
                $"{request.Scheme}://{request.Host}/api/v1/Admin/users/preview?avatar=avatar_{userId}_{userAdminDto.file.FileName}";
            updateUser.Avatar = userAdminDto.file.FileName;
        }
        else if (userAdminDto.file == null && string.IsNullOrEmpty(userAdminDto.Avatar))
        {
            await _cloudflareClient.DeleteObjectAsync($"avatar_{updateUser.UserId}_{updateUser.Avatar}");
            updateUser.Avatar = string.Empty;
            updateUser.Url = string.Empty;
        }


        updateUser.UserName = userAdminDto.UserName;
        updateUser.Email = userAdminDto.Email;
        updateUser.Role = userAdminDto.Role;
        updateUser.Phone = userAdminDto.Phone ?? "";
        updateUser.BirthDate = Convert.ToDateTime(userAdminDto.BirthDate.ToLongDateString());
        updateUser.ModifiedAt = DateTime.Now;
        if (!string.IsNullOrEmpty(userAdminDto.Password))
        {
            updateUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userAdminDto.Password);
        }

        updateUser.EmailConfirm = userAdminDto.EmailConfirm;
        updateUser.IsActive = userAdminDto.IsActive;
        await _context.SaveChangesAsync();

        return updateUser;
    }
}