using System.Net;
using EcommerceApi.Config;
//using BCrypt.Net;
using EcommerceApi.Constant;
using EcommerceApi.Dtos.Admin;
using EcommerceApi.Dtos.Upload;
using EcommerceApi.Dtos.User;
using EcommerceApi.ExtensionExceptions;
using EcommerceApi.FilterBuilder;
using EcommerceApi.Models;
using EcommerceApi.Models.Segment;
using EcommerceApi.Models.UserAddress;
using EcommerceApi.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SortOrder = EcommerceApi.Constant.SortOrder;

namespace EcommerceApi.Services;

public class UserService : IUserService
{
    private readonly EcommerceDbContext _context;
    private readonly ICloudflareClientService _cloudflareClient;
    private readonly UserFilterBuilder _userFilterBuilder;
    private readonly CloudflareR2Config _cloudFlareOption;

    public UserService(EcommerceDbContext context, 
        ICloudflareClientService cloudflareClient, 
        IOptions<CloudflareR2Config> cloudFlareOption,
        UserFilterBuilder userFilterBuilder)
    {
        _context = context;
        _cloudflareClient = cloudflareClient;
        _userFilterBuilder = userFilterBuilder;
        _cloudFlareOption = cloudFlareOption.Value;
    }

    public async Task<User> PostUserAsync(UserAdminDto userAdmin, CancellationToken userCancellationToken)
    {
        try
        {
            if (_context.Users.AsNoTracking().Where(u => u.UserName == userAdmin.UserName || u.Email == userAdmin.Email)
                .Any())
            {
                throw new HttpStatusException(HttpStatusCode.Conflict,
                    "The request could not be completed due to a conflict with the current state of the resource.");
            }

            var newUser = new User()
            {
                UserId = userAdmin.UserId,
                UserName = userAdmin.UserName,
                Email = userAdmin.Email,
                CreatedAt = DateTime.Now,
                ModifiedAt = DateTime.Now,
                Role = userAdmin.Role.ToLower(),
                Phone = userAdmin.Phone!,
                BirthDate = userAdmin.BirthDate,
                IsActive = userAdmin.IsActive,
                EmailConfirm = userAdmin.EmailConfirm,
                Avatar = userAdmin.Avatar,
                Url = userAdmin.Url,
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
        catch (HttpStatusException hse)
        {
            throw new HttpStatusException(hse.Status, hse.Message);
        }
        catch (Exception ex)
        {
            throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    public async Task<bool> GetUserByUserNameAync(string userName, CancellationToken userCancellationToken)
    {
        try
        {
            var isExist = await _context.Users.AnyAsync(u => u.UserName.ToLower().Equals(userName.ToLower()), userCancellationToken);
            return isExist;
        }
        catch(Exception ex) {
            throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    public async Task<User?> GetUserByIdAsync(Guid userId, CancellationToken userCancellationToken)
    {
        try
        {
            var user = await _context
                                    .Users
                                    .Where(u => u.UserId == userId)
                                    .FirstOrDefaultAsync(userCancellationToken)
                                    ?? throw new HttpStatusException(HttpStatusCode.NotFound, "User not found.");

            return user;
        }
        catch (Exception ex)
        {
            throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    public async Task<List<UserResponse>> GetListUsersAsync(string? sort, string? range, string? filter,
        HttpResponse response, CancellationToken userCancellationToken)
    {
        try
        {
            var rangeValues = Helpers.ParseString<int>(range);

            if (rangeValues.Count == 0)
            {
                rangeValues.AddRange(new List<int> { 0, 24 });
            };

            var sortValues = Helpers.ParseString<string>(sort);

            if (sortValues.Count == 0)
            {
                sortValues.AddRange(new List<string> { "", "" });
            }

            var filterValues = Helpers.ParseString<string>(filter);
            if (!filterValues.Contains(UserFilterType.Search))
            {
                filterValues.Insert(0, UserFilterType.Search);
                filterValues.Insert(1, "");
            }
            else
            {
                var search = filterValues.IndexOf(UserFilterType.Search) + 1;
                filterValues.Insert(0, filterValues[search]);
                filterValues.Insert(0, "q");

                if (filterValues[^1] == "q")
                {
                    filterValues.RemoveAt(filterValues.LastIndexOf(UserFilterType.Search) - 1);
                    filterValues.RemoveAt(filterValues.LastIndexOf(UserFilterType.Search));
                }
                else
                {
                    filterValues.RemoveAt(filterValues.LastIndexOf(UserFilterType.Search) + 1);
                    filterValues.RemoveAt(filterValues.LastIndexOf(UserFilterType.Search));
                }
            }

            if (!filterValues.Contains(UserFilterType.Segments))
            {
                filterValues.Add(UserFilterType.Segments);
                filterValues.Add("");
            }


            if (!filterValues.Contains(UserFilterType.IsVerify))
            {
                filterValues.Add(UserFilterType.IsVerify);
                filterValues.Add("");
            }

            if (!filterValues.Contains(UserFilterType.IsActive))
            {
                filterValues.Add(UserFilterType.IsActive);
                filterValues.Add("");
            }
            else
            {
                var indexActive = filterValues.IndexOf(UserFilterType.IsActive);
                filterValues.Add(UserFilterType.IsActive);
                filterValues.Add(filterValues[indexActive + 1]);
                filterValues.Remove(UserFilterType.IsActive);
                filterValues.Remove(filterValues[indexActive]);
            }

            var perPage = rangeValues[1] - rangeValues[0] + 1;
            var currentPage = Convert.ToInt32(Math.Ceiling((double)rangeValues[0] / perPage)) + 1;
            var sortBy = sortValues[0].ToLower();
            var sortType = sortValues[1].ToLower();

            var listUsersQuery = _context
                .Users
                .Where(u => u.Role != "admin")
                .Include(u => u.UserAddresses)
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
                    }).ToList(),
                    UserAddresses = u.UserAddresses.ToList()
                });

            var listUsers = await listUsersQuery
                .AsNoTracking()
                .ToListAsync(userCancellationToken);

            var filterBan = filterValues[filterValues.IndexOf(UserFilterType.IsActive) + 1];
            var filterSegment = filterValues[filterValues.IndexOf(UserFilterType.Segments) + 1];
            var filterSearch = filterValues[filterValues.IndexOf(UserFilterType.Search) + 1];
            var filterVerify = filterValues[filterValues.IndexOf(UserFilterType.IsVerify) + 1];

            var filterRefIds = new List<Guid>();
            if (filterValues.Contains(UserFilterType.Id))
            {
                var keyStartIndex = filterValues.IndexOf(UserFilterType.Id);
                var keyEndIndex = filterValues.IndexOf(UserFilterType.Segments);
                var listId = filterValues
                    .Skip(keyStartIndex + 1)
                    .Take(keyEndIndex - keyStartIndex - 1)
                    .Select(Guid.Parse).ToList();
                filterRefIds.AddRange(listId);
                listUsers = listUsers
                    .Where(u => filterRefIds.Contains(u.Id))
                    .ToList();
            }

            var filters = _userFilterBuilder
                                            .AddBanFilter(filterBan)
                                            .AddVerifyFilter(filterVerify)
                                            .AddSegmentsFilter(GetSegments(filterValues))
                                            .Build();
            listUsers = listUsers
                                .Where(filters)
                                .ToList();
            {
            //    listUsers = listUsers
            //    .Where(u => filterSegment != "" && filterSearch != "" && filterBan != ""
            //        ? u.UserName.ToLower().Contains(filterSearch.ToLower()) &&
            //          IsExistSegment(u.Segments, filterValues) && u.IsActive.ToString().ToLower() == filterBan.ToLower()
            //        : filterSegment == "" && filterSearch == "" && filterBan == ""
            //            ? filterSegment == "" && filterSearch == "" && filterBan == ""
            //            : filterSearch != "" && filterSegment == "" && filterBan == ""
            //                ? u.UserName.ToLower().Contains(filterSearch.ToLower())
            //                : filterSearch == "" && filterSegment != "" && filterBan == ""
            //                    ? IsExistSegment(u.Segments, filterValues)
            //                    : filterSearch == "" && filterSegment == "" && filterBan != ""
            //                        ? u.IsActive.ToString().ToLower() == filterBan.ToLower()
            //                        : filterSearch != "" && filterSegment != "" && filterBan == ""
            //                            ? u.UserName.ToLower().Contains(filterSearch.ToLower()) &&
            //                              IsExistSegment(u.Segments, filterValues)
            //                            : filterSearch == "" && filterSegment != "" && filterBan != ""
            //                                ? IsExistSegment(u.Segments, filterValues) &&
            //                                  u.IsActive.ToString().ToLower() == filterBan.ToLower()
            //                                : u.UserName.ToLower().Contains(filterSearch.ToLower()) &&
            //                                  u.IsActive.ToString().ToLower() == filterBan.ToLower()
            //    ).ToList();
            }

            listUsers = sortType switch
            {
                "asc" => sortBy switch
                {
                    SortOrder.SortById => listUsers.OrderBy(u => u.Id).ToList(),
                    SortOrder.SortByBirthDate => listUsers.OrderBy(u => u.BirthDate).ToList(),
                    _ => listUsers
                },
                "desc" => sortBy switch
                {
                    SortOrder.SortById => listUsers.OrderByDescending(u => u.Id).ToList(),
                    SortOrder.SortByBirthDate => listUsers.OrderByDescending(u => u.BirthDate).ToList(),
                    _ => listUsers
                },
                _ => listUsers
            };

            {
                //var totalUser = listUsers.Count;

                //listUsers = listUsers
                //    .Skip((currentPage - 1) * perPage)
                //    .Take(perPage)
                //    .ToList();

                //response.Headers.Append("Access-Control-Expose-Headers", "Content-Range");
                //response.Headers.Append("Content-Range", $"users {rangeValues[0]}-{rangeValues[1]}/{totalUser}");
                //return listUsers;
            }
            var listUserPaging = Helpers.CreatePaging(listUsers, rangeValues, currentPage, perPage, "users", response);
            return listUserPaging;
        }
        catch (SqlException ex)
        {
            throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
        }
    }

    private List<string> GetSegments(List<string> segmentFilter)
    {
        var filters = segmentFilter
            .Skip(segmentFilter.IndexOf(UserFilterType.Segments) + 1)
            .Take(segmentFilter.Count)
            .ToList();
        if (filters.Contains(UserFilterType.IsActive))
        {
            filters.RemoveAt(filters.IndexOf(UserFilterType.IsActive) + 1);
            filters.RemoveAt(filters.IndexOf(UserFilterType.IsActive));
        }
        if (filters.Contains(UserFilterType.IsVerify))
        {
            filters.RemoveAt(filters.IndexOf(UserFilterType.IsVerify) + 1);
            filters.RemoveAt(filters.IndexOf(UserFilterType.IsVerify));
        }

        return filters;
    }

    public async Task<bool> DeleteUserByIdAsync(Guid userId, CancellationToken userCancellationToken)
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
        catch (SqlException ex)
        {
            throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
        }
    }
    public async Task<UserResponse> UpdateUserProfile(Guid userId, HttpRequest request, UserProfileDto userProfileDto, CancellationToken userCancellationToken)
    {
        try
        {
            var isExistedPhone = await _context
                                               .Users
                                               .AnyAsync(u => u.Phone.Equals(userProfileDto.Phone) && !u.UserId.Equals(userId), userCancellationToken);
            if (isExistedPhone) throw new HttpStatusException(HttpStatusCode.Conflict, "The phone number is already used by another account.");

            var userProfile = await _context
                                            .Users
                                            .Where(u => u.UserId.Equals(userId))
                                            .FirstOrDefaultAsync(userCancellationToken)
                                            ?? throw new HttpStatusException(HttpStatusCode.NotFound, "User not found.");

            if (userProfileDto?.Avatar?.FileName != userProfile.Avatar && userProfileDto?.Avatar is not null)
            {
                var res = await _cloudflareClient.DeleteObjectAsync(
                       $"avatar_{userProfile.UserId}_{userProfile.Avatar}", userCancellationToken);
                if (res.HttpStatusCode == HttpStatusCode.NoContent)
                {
                    await _cloudflareClient.UploadImageAsync(new UploadDto()
                    {
                        Id = userId,
                        File = userProfileDto!.Avatar,
                    }, "avatar", userCancellationToken);
                }
                userProfile.Url = $"{_cloudFlareOption.publicUrl}/avatar_{userId}_{userProfileDto!.Avatar.FileName}";
                userProfile.Avatar = userProfileDto.Avatar.FileName;
            }

            if(!string.IsNullOrEmpty(userProfileDto.Email) && userProfileDto.Email != userProfile.Email)
            {
                userProfile.EmailConfirm = false;
                userProfile.Email = userProfileDto.Email;
            }

            userProfile.UserName = userProfileDto.UserName;
            userProfile.Phone = userProfileDto.Phone;
            userProfile.BirthDate = Convert.ToDateTime(userProfileDto.BirthDate.ToLongDateString());

            await _context.SaveChangesAsync(userCancellationToken);

            return new UserResponse()
            {
                Id = userProfile.UserId,
                UserName = userProfile.UserName,
                Email = userProfile.Email,
                Avatar = userProfile.Avatar,
                BirthDate = userProfile.BirthDate,
                EmailConfirm = userProfile.EmailConfirm,
                IsActive = userProfile.IsActive,
                Phone = userProfile.Phone,
                Role = userProfile.Role.ToLower(),
                Url = userProfile.Url,
            };

        }
        catch (HttpStatusException hse)
        {
            throw new HttpStatusException(hse.Status, hse.Message);
        }
        catch (Exception ex)
        {
            throw new HttpStatusException(HttpStatusCode.BadRequest, ex.Message);
        }
    }

    public async Task<User> UpdateUserByIdAsync(Guid userId, UserAdminDto userAdminDto, HttpContext httpContext, HttpRequest request,
        CancellationToken userCancellationToken)
    {
        try
        {
            var updateUser = await this.GetUserByIdAsync(userId, userCancellationToken);
            if (updateUser == null)
            {
                throw new HttpStatusException(HttpStatusCode.NotFound, "User not found.");
            };
            if (_context.Users
                .Where(u => (u.UserName == userAdminDto.UserName || u.Email == userAdminDto.Email) &&
                            u.UserId != userId)
                .Any())
            {
                throw new HttpStatusException(HttpStatusCode.Conflict,
                    "The request could not be completed due to a conflict with the current state of the resource.");
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
                    var res = await _cloudflareClient.DeleteObjectAsync(
                        $"avatar_{updateUser.UserId}_{updateUser.Avatar}", userCancellationToken);
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
                    $"{_cloudFlareOption.publicUrl}/avatar_{userId}_{userAdminDto.file.FileName}";
                updateUser.Avatar = userAdminDto.file.FileName;
            }
            else if (userAdminDto.file == null && string.IsNullOrEmpty(userAdminDto.Avatar))
            {
                await _cloudflareClient.DeleteObjectAsync($"avatar_{updateUser.UserId}_{updateUser.Avatar}",
                    userCancellationToken);
                updateUser.Avatar = string.Empty;
                updateUser.Url = string.Empty;
            }

            //var currentUserRole = Helpers.GetUserRoleLogin(httpContext);
            //if (currentUserRole == "admin") { 
            //    updateUser.Role = userAdminDto.Role;
            //}

            updateUser.UserName = userAdminDto.UserName;
            updateUser.Email = userAdminDto.Email;
            updateUser.Phone = userAdminDto.Phone ?? "";
            updateUser.BirthDate = Convert.ToDateTime(userAdminDto.BirthDate.ToLongDateString());
            updateUser.ModifiedAt = DateTime.Now;
            updateUser.EmailConfirm = userAdminDto.EmailConfirm;
            updateUser.IsActive = userAdminDto.IsActive;
            await _context.SaveChangesAsync(userCancellationToken);

            return updateUser;
        }
        catch (Exception ex)
        {
            throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    public async Task<FileStreamResult> GetAvatarAsync(string avatarUrl, CancellationToken userCancellationToken)
    {
        var response = await _cloudflareClient.GetObjectAsync(avatarUrl, userCancellationToken);
        if (response.HttpStatusCode == HttpStatusCode.OK)
        {
            return new FileStreamResult(response.ResponseStream, response.Headers.ContentType)
            {
                FileDownloadName = avatarUrl
            };
        }

        throw new HttpStatusException(response.HttpStatusCode, "Avatar not found.");
    }

    public async Task<bool> UpdateUserConfirm(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _context.Users
                                           .Where(u => u.UserId.Equals(userId))
                                           .FirstOrDefaultAsync(cancellationToken)
                                           ?? throw new HttpStatusException(HttpStatusCode.NotFound, "User not found.");
            user.EmailConfirm = true;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {      
            throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
        }
    }
}