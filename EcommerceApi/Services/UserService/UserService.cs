using System.Linq;
using System.Net;
using EcommerceApi.Dtos.Admin;
using EcommerceApi.Dtos.Upload;
using EcommerceApi.ExtensionExceptions;
using EcommerceApi.Models;
using EcommerceApi.Models.Segment;
using EcommerceApi.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApi.Services;

public class UserService : IUserService
{
    private readonly EcommerceDbContext _context;
    private readonly ICloudflareClientService _cloudflareClient;

    public UserService(EcommerceDbContext context, ICloudflareClientService cloudflareClient)
    {
        _context = context;
        _cloudflareClient = cloudflareClient;
    }

    public async Task<User> PostUserAsync(UserAdminDto userAdmin, CancellationToken userCancellationToken)
    {
        try
        {
            if (_context.Users.AsNoTracking().Where(u => u.UserName == userAdmin.UserName || u.Email == userAdmin.Email)
            .Any())
            {
                throw new HttpStatusException(HttpStatusCode.Conflict, "The request could not be completed due to a conflict with the current state of the resource.");
            }
            var newUser = new User()
            {
                UserName = userAdmin.UserName,
                Email = userAdmin.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userAdmin.Password),
                CreatedAt = DateTime.Now,
                ModifiedAt = DateTime.Now,
                Role = userAdmin.Role,
                Phone = userAdmin.Phone!,
                BirthDate = userAdmin.BirthDate,
                IsActive = userAdmin.IsActive,
                EmailConfirm = userAdmin.EmailConfirm,
            };
            await _context.Users.AddAsync(newUser, userCancellationToken);
            await _context.SaveChangesAsync(userCancellationToken);
            var userResponse = await _context
                                            .Users
                                            .AsNoTracking()
                                            .Where(u => u.UserName == newUser.UserName)
                                            .FirstOrDefaultAsync(userCancellationToken);
            return userResponse!;
        }
        catch(SqlException ex)
        {
            throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
        }
    }

    public async Task<User?> GetUserByIdAsync(int userId, CancellationToken userCancellationToken)
    {
        try
        {
            var user = await _context.Users.FindAsync(new object[] { userId }, userCancellationToken);
            if (user == null)
            {
                throw new HttpStatusException(HttpStatusCode.NotFound, "User not found.");
            }
            return user;
        }
        catch(SqlException ex)
        {
            throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
        }
    }

    public async Task<List<UserResponse>> GetListUsersAsync(string sort, string range, string filter,
        HttpResponse response, CancellationToken userCancellationToken)
    {
        try
        {
            List<int> rangeValues = Helpers.ParseString<int>(range);

            if (rangeValues.Count == 0)
            {
                rangeValues.AddRange(new List<int> { 0, 4 });
            };

            List<string> sortValues = Helpers.ParseString<string>(sort);

            if (sortValues.Count == 0)
            {
                sortValues.AddRange(new List<string> { "", "" });
            }

            List<string> filterValues = Helpers.ParseString<string>(filter);
            if (!filterValues.Contains("q"))
            {
                filterValues.Insert(0, "q");
                filterValues.Insert(1, "");
            }
            else
            {
                var search = filterValues.IndexOf("q") + 1;
                filterValues.Insert(0, filterValues[search]);
                filterValues.Insert(0, "q");

                if (filterValues[filterValues.Count - 1] == "q")
                {
                    filterValues.RemoveAt(filterValues.LastIndexOf("q") - 1);
                    filterValues.RemoveAt(filterValues.LastIndexOf("q"));
                }
                else
                {
                    filterValues.RemoveAt(filterValues.LastIndexOf("q") + 1);
                    filterValues.RemoveAt(filterValues.LastIndexOf("q"));
                }
            }

            if (!filterValues.Contains("segments"))
            {
                filterValues.Add("segments");
                filterValues.Add("");
            }

            if (!filterValues.Contains("isActive"))
            {
                filterValues.Add("isActive");
                filterValues.Add("");
            }
            else
            {
                var indexActive = filterValues.IndexOf("isActive");
                filterValues.Add("isActive");
                filterValues.Add(filterValues[indexActive + 1]);
                filterValues.Remove("isActive");
                filterValues.Remove(filterValues[indexActive]);
            }

            var perPage = rangeValues[1] - rangeValues[0] + 1;
            var currentPage = Convert.ToInt32(Math.Ceiling((double)rangeValues[0] / perPage)) + 1;
            var sortBy = sortValues[0].ToLower();
            var sortType = sortValues[1].ToLower();

            var listUsers = await _context
                                          .Users
                                          .Where(u => u.Role != "admin")
                                          .Include(u => u.UserSegments)
                                          .ThenInclude(u => u.Segment)
                                          .Select(u => new UserResponse()
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
                                               Segments = u.UserSegments.Select(us => new Segment
                                               {
                                                   SegmentId = us.Segment.SegmentId,
                                                   Title = us.Segment.Title,
                                               }).ToList()
                                          })
                                          .ToListAsync(userCancellationToken);

            var totalUser = listUsers.Count;

            var filterBan = filterValues[filterValues.IndexOf("isActive") + 1];

            listUsers = listUsers
                                .Where(u => filterValues[3] != "" && filterValues[1] != "" && filterBan != ""
                                        ? u.UserName.ToLower().Contains(filterValues[1].ToLower()) &&
                                        IsExistSegment(u.Segments, filterValues) && u.IsActive.ToString().ToLower() == filterBan.ToLower()
                                        : filterValues[3] == "" && filterValues[1] == "" && filterBan == ""
                                            ? filterValues[3] == "" && filterValues[1] == "" && filterBan == ""
                                            : filterValues[1] != "" && filterValues[3] == "" && filterBan == ""
                                                ? u.UserName.ToLower().Contains(filterValues[1].ToLower())
                                                : filterValues[1] == "" && filterValues[3] != "" && filterBan == "" 
                                                ? IsExistSegment(u.Segments, filterValues)
                                                : filterValues[1] == "" && filterValues[3] == "" && filterBan != ""
                                                ? u.IsActive.ToString().ToLower() == filterBan.ToLower()
                                                : filterValues[1] != "" && filterValues[3] != "" && filterBan == ""
                                                ? u.UserName.ToLower().Contains(filterValues[1].ToLower()) && IsExistSegment(u.Segments, filterValues)
                                                : filterValues[1] == "" && filterValues[3] != "" && filterBan != ""
                                                ? IsExistSegment(u.Segments, filterValues) && u.IsActive.ToString().ToLower() == filterBan.ToLower()
                                                : u.UserName.ToLower().Contains(filterValues[1].ToLower()) && u.IsActive.ToString().ToLower() == filterBan.ToLower()
                                    )
                                .Skip((currentPage - 1) * perPage)
                                .Take(perPage)
                                .ToList();

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

            response.Headers.Append("Access-Control-Expose-Headers", "Content-Range");
            response.Headers.Append("Content-Range", $"users {rangeValues[0]}-{rangeValues[1]}/{totalUser}");
            return listUsers;
        }
        catch(SqlException ex)
        {
            throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
        }
    }

    private bool IsExistSegment(List<Segment> segments, List<string> segmentFitler) {

        var filters = segmentFitler
                                   .Skip(segmentFitler.IndexOf("segments") + 1)
                                   .Take(segmentFitler.Count)
                                   .ToList();
        if(filters.Contains("isActive"))
        {
            filters.RemoveAt(filters.IndexOf("isActive") + 1);
            filters.RemoveAt(filters.IndexOf("isActive"));
        }

        int elmCount = 0;
        foreach(var us in segments) // review, order
        {
            foreach(var f in filters) // review
            {
                if (f.ToLower() == us.Title.ToLower())
                {
                    elmCount++;
                    break;
                } 
            }
        }
        return elmCount >= filters.Count();
    }

    public async Task<bool> DeleteUserByIdAsync(int userId, CancellationToken userCancellationToken)
    {
        try
        {
            var deleteUser = await this.GetUserByIdAsync(userId, userCancellationToken);
            if (deleteUser == null)
            {
                throw new HttpStatusException(HttpStatusCode.NotFound, "User not found.");
            }
            _context.Users.Remove(deleteUser);
            await _context.SaveChangesAsync(userCancellationToken);
            return true;
        }
        catch(SqlException ex)
        {
            throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
        }
    }

    public async Task<User> UpdateUserByIdAsync(int userId, UserAdminDto userAdminDto, HttpRequest request, CancellationToken userCancellationToken)
    {
        try
        {
            var updateUser = await this.GetUserByIdAsync(userId, userCancellationToken);
            if (updateUser == null)
            {
                throw new HttpStatusException(HttpStatusCode.NotFound, "User not found.");
            };
            if (_context.Users
                .Where(u => (u.UserName == userAdminDto.UserName || u.Email == userAdminDto.Email) && u.UserId != userId)
                .Any())
            {
                throw new HttpStatusException(HttpStatusCode.Conflict, "The request could not be completed due to a conflict with the current state of the resource.");
            }

            if (userAdminDto.file != null && userAdminDto.Avatar != "")
            {
                if (string.IsNullOrEmpty(updateUser.Url))
                {
                    await _cloudflareClient.UploadImageAsync(new UploadDto()
                    {
                        Id = updateUser.UserId,
                        File = userAdminDto.file,
                    }, "avatar", userCancellationToken);
                }
                else
                {
                    var res = await _cloudflareClient.DeleteObjectAsync($"avatar_{updateUser.UserId}_{updateUser.Avatar}", userCancellationToken);
                    if (res.HttpStatusCode == HttpStatusCode.NoContent)
                    {
                        await _cloudflareClient.UploadImageAsync(new UploadDto()
                        {
                            Id = updateUser.UserId,
                            File = userAdminDto.file,
                        }, "avatar", userCancellationToken);
                    }
                }

                updateUser.Url =
                    $"{request.Scheme}://{request.Host}/api/v1/Admin/users/preview?avatar=avatar_{userId}_{userAdminDto.file.FileName}";
                updateUser.Avatar = userAdminDto.file.FileName;
            }
            else if (userAdminDto.file == null && string.IsNullOrEmpty(userAdminDto.Avatar))
            {
                await _cloudflareClient.DeleteObjectAsync($"avatar_{updateUser.UserId}_{updateUser.Avatar}", userCancellationToken);
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
            await _context.SaveChangesAsync(userCancellationToken);

            return updateUser;
        }
        catch(SqlException ex) {
            throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
        }
    }

    public async Task<FileStreamResult> GetAvatarAsync(string avatarUrl, CancellationToken userCancellationToken)
    {
        var response = await _cloudflareClient.GetObjectAsync(avatarUrl, userCancellationToken);
        if(response.HttpStatusCode == HttpStatusCode.OK)
        {
            return new FileStreamResult(response.ResponseStream, response.Headers.ContentType) { 
                FileDownloadName = avatarUrl
            };
        }
        throw new HttpStatusException(response.HttpStatusCode, "Avatar not found.");
    }
}