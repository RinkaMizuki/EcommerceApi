using EcommerceApi.Dtos.Admin;
using EcommerceApi.ExtensionExceptions;
using EcommerceApi.Models;
using EcommerceApi.Models.Rate;
using EcommerceApi.Models.Segment;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Net;
using EcommerceApi.Constant;
using EcommerceApi.FilterBuilder;
using SortOrder = EcommerceApi.Constant.SortOrder;

namespace EcommerceApi.Services.FeedbackService
{
    public class RateService : IRateService
    {
        private readonly EcommerceDbContext _context;
        private readonly RateFilterBuilder _rateFilterBuilder;

        public RateService(EcommerceDbContext context, RateFilterBuilder rateFilterBuilder)
        {
            _context = context;
            _rateFilterBuilder = rateFilterBuilder;
        }

        public async Task<Rate> GetRateByIdAsync(int rateId, CancellationToken userCancellationToken)
        {
            try
            {
                var rateById = await _context
                                   .Rates
                                   .Where(r => r.RateId == rateId)
                                   .FirstOrDefaultAsync(userCancellationToken)
                               ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Rating not found.");

                return rateById;
            }
            catch (Exception ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<bool> DeleteRateAsync(int rateId, CancellationToken userCancellationToken)
        {
            try
            {
                var rateById = await _context
                                   .Rates
                                   .Where(r => r.RateId == rateId)
                                   .FirstOrDefaultAsync(userCancellationToken)
                               ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Rate not found.");
                _context.Rates.Remove(rateById);
                await _context.SaveChangesAsync(userCancellationToken);
                return true;
            }
            catch (SqlException ex)
            {
                throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
            }
        }

        public async Task<List<Rate>> GetListRateAsync(string sort, string range, string filter, HttpResponse response,
            CancellationToken userCancellationToken)
        {
            try
            {
                var rangeValues = Helpers.ParseString<int>(range);

                if (rangeValues.Count == 0)
                {
                    rangeValues.AddRange(new List<int> { 0, 4 });
                }

                var sortValues = Helpers.ParseString<string>(sort);

                if (sortValues.Count == 0)
                {
                    sortValues.AddRange(new List<string> { "", "" });
                }

                var filterValues = Helpers.ParseString<string>(filter);

                if (!filterValues.Contains(RateFilterType.Search))
                {
                    filterValues.Add(RateFilterType.Search);
                    filterValues.Add("");
                }

                if (!filterValues.Contains(RateFilterType.Status))
                {
                    filterValues.Add(RateFilterType.Status);
                    filterValues.Add("");
                }

                if (!filterValues.Contains(RateFilterType.UserId))
                {
                    filterValues.Add(RateFilterType.UserId);
                    filterValues.Add("");
                }

                if (!filterValues.Contains(RateFilterType.ProductId))
                {
                    filterValues.Add(RateFilterType.ProductId);
                    filterValues.Add("");
                }

                if (!filterValues.Contains(RateFilterType.Before))
                {
                    filterValues.Add(RateFilterType.Before);
                    filterValues.Add("");
                }

                if (!filterValues.Contains(RateFilterType.Since))
                {
                    filterValues.Add(RateFilterType.Since);
                    filterValues.Add("");
                }

                var perPage = rangeValues[1] - rangeValues[0] + 1;
                var currentPage = Convert.ToInt32(Math.Ceiling((double)rangeValues[0] / perPage)) + 1;

                var sortBy = sortValues[0].ToLower();
                var sortType = sortValues[1].ToLower();

                var listRateQuery = _context
                    .Rates
                    .Include(r => r.Product)
                    .Include(r => r.User);

                var listRate = await listRateQuery
                    .AsNoTracking()
                    .ToListAsync(userCancellationToken);

                //business logic for filter or sort
                //....
                var filterSearch = filterValues[filterValues.IndexOf(RateFilterType.Search) + 1];
                var filterStatus = filterValues[filterValues.IndexOf(RateFilterType.Status) + 1];
                var filterUser = filterValues[filterValues.IndexOf(RateFilterType.UserId) + 1];
                var filterProduct = filterValues[filterValues.IndexOf(RateFilterType.ProductId) + 1];
                var filterBefore = filterValues[filterValues.IndexOf(RateFilterType.Before) + 1];
                var filterSince = filterValues[filterValues.IndexOf(RateFilterType.Since) + 1];

                var filters = _rateFilterBuilder
                                                .AddSearchFilter(filterSearch)
                                                .AddStatusFilter(filterStatus)
                                                .AddUserFilter(filterUser)
                                                .AddProductFilter(filterProduct)
                                                .AddBeforeDateFilter(filterBefore)
                                                .AddSinceDateFilter(filterSince)
                                                .Build();

                listRate = listRate.Where(filters).ToList();

                listRate = sortType switch
                {
                    "asc" => sortBy switch
                    {
                        SortOrder.SortByStar => listRate.OrderBy(r => r.Star).ToList(),
                        SortOrder.SortByUser => listRate.OrderBy(r => r.User.UserName).ToList(),
                        SortOrder.SortByCreatedAt => listRate.OrderBy(r => r.CreatedAt).ToList(),
                        SortOrder.SortByProduct => listRate.OrderBy(r => r.Product.Title).ToList(),
                        _ => listRate
                    },
                    "desc" => sortBy switch
                    {
                        SortOrder.SortByStar => listRate.OrderByDescending(r => r.Star).ToList(),
                        SortOrder.SortByUser => listRate.OrderByDescending(r => r.User.UserName).ToList(),
                        SortOrder.SortByCreatedAt => listRate.OrderByDescending(r => r.CreatedAt).ToList(),
                        SortOrder.SortByProduct => listRate.OrderByDescending(r => r.Product.Title).ToList(),
                        _ => listRate
                    },
                    _ => listRate.OrderByDescending(r => r.CreatedAt).ToList()
                };

                var totalRate = listRate.Count;

                listRate = listRate
                    .Skip((currentPage - 1) * perPage)
                    .Take(perPage)
                    .ToList();

                var counter = 0;
                foreach (var l in filterValues)
                {
                    if (l.Equals("")) counter++;
                }

                if (counter < 6)
                {
                    totalRate = listRate.Count;
                }

                response.Headers.Append("Access-Control-Expose-Headers", "Content-Range");
                response.Headers.Append("Content-Range", $"rates {rangeValues[0]}-{rangeValues[1]}/{totalRate}");
                return listRate;
            }
            catch (Exception ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<Rate> PostRateAsync(RateDto rateDto, CancellationToken userCancellationToken)
        {
            try
            {
                var productRate = await _context
                    .Products
                    .Where(p => p.ProductId == rateDto.ProductId)
                    .Include(p => p.ProductRates)
                    .FirstOrDefaultAsync(userCancellationToken);
                var userRate = await _context
                    .Users
                    .Include(u => u.UserSegments)
                    .ThenInclude(us => us.Segment)
                    .Where(u => u.UserId == rateDto.UserId)
                    .FirstOrDefaultAsync(userCancellationToken);
                if (productRate is null)
                {
                    throw new HttpStatusException(HttpStatusCode.NotFound, "Product not found.");
                }

                if (userRate is null)
                {
                    throw new HttpStatusException(HttpStatusCode.NotFound, "User not found.");
                }

                if (productRate.ProductRates.Where(pr => pr.UserId == userRate.UserId).Any())
                {
                    throw new HttpStatusException(HttpStatusCode.BadRequest, "This product has been rated.");
                }

                var newRate = new Rate()
                {
                    Content = rateDto.Content,
                    Star = rateDto.Star,
                    Status = "pending",
                    CreatedAt = DateTime.Now,
                    ModifiedAt = DateTime.Now,
                    Product = productRate,
                    ProductId = productRate.ProductId,
                    User = userRate,
                    UserId = userRate.UserId,
                };

                bool flag = false;

                foreach (var s in userRate.UserSegments)
                {
                    if (s.Segment.Title == "Review" && s.UserId == userRate.UserId)
                    {
                        flag = true;
                        break;
                    }
                }

                var segment = await _context
                                  .Segments
                                  .Where(s => s.Title == "Review")
                                  .FirstOrDefaultAsync(userCancellationToken)
                              ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Segment not found.");

                if (!flag)
                {
                    var newUserSegment = new UserSegment()
                    {
                        SegmentId = segment!.SegmentId,
                        UserId = userRate.UserId,
                        User = userRate,
                    };
                    await _context.UserSegments.AddAsync(newUserSegment, userCancellationToken);
                }

                await _context
                    .Rates
                    .AddAsync(newRate, userCancellationToken);

                await _context.SaveChangesAsync(userCancellationToken);

                return newRate;
            }
            catch (SqlException ex)
            {
                throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
            }
        }

        public async Task<Rate> UpdateRateAsync(RateDto rateDto, int rateId, CancellationToken userCancellationToken)
        {
            try
            {
                var updateRate = await _context
                                     .Rates
                                     .Where(r => r.RateId == rateId)
                                     .FirstOrDefaultAsync(userCancellationToken)
                                 ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Rate not found.");

                updateRate.Content = rateDto.Content;
                updateRate.Star = rateDto.Star;
                updateRate.ModifiedAt = DateTime.Now;
                updateRate.Status = rateDto.Status;

                await _context.SaveChangesAsync(userCancellationToken);
                return updateRate;
            }
            catch (SqlException ex)
            {
                throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
            }
        }
    }
}