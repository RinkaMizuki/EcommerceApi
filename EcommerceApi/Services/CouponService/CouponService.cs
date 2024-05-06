using Azure;
using EcommerceApi.Dtos.Admin;
using EcommerceApi.Dtos.User;
using EcommerceApi.ExtensionExceptions;
using EcommerceApi.Models;
using EcommerceApi.Models.Coupon;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace EcommerceApi.Services.CouponService
{
    public class CouponService : ICouponService
    {
        private readonly EcommerceDbContext _context;

        public CouponService(EcommerceDbContext context)
        {
            _context = context;
        }

        public async Task<object> ApplyCouponProductAsync(CouponProductDto couponProductDto, CancellationToken userCancellationToken)
        {
            var coupon = await _context
                                       .Coupons
                                       .Where(c => c.CouponCode.ToUpper().Equals(couponProductDto.CouponCode))
                                       .Include(c => c.CouponConditions)
                                       .ThenInclude(cc => cc.Condition)
                                       .FirstOrDefaultAsync(userCancellationToken)
                                       ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Coupon not found.");
            try
            {

                var productIds = couponProductDto.ListProductInfo.Select(x => x.ProductId).ToList();

                var listProduct = await _context
                                        .Products
                                        .AsNoTracking()
                                        .Where(p => productIds.Contains(p.ProductId))
                                        .ToListAsync(userCancellationToken);

                var total = Convert.ToInt32(listProduct.Sum(p =>
                    Convert.ToInt32(p.Price) * couponProductDto.ListProductInfo.FirstOrDefault(x => x.ProductId.Equals(p.ProductId))!.Quantity * (1 - (float)p.Discount / 100)
                ));

                var minAmount = Convert.ToInt32(coupon.CouponConditions.Where(cc => cc.Condition.Attribute == "min_amount").FirstOrDefault()?.Value ?? 0);
                var maxAmount = Convert.ToInt32(coupon.CouponConditions.Where(cc => cc.Condition.Attribute == "max_amount").FirstOrDefault()?.Value ?? 0);
                var maxDiscount = Convert.ToInt32(coupon.CouponConditions.Where(cc => cc.Condition.Attribute == "max_discount").FirstOrDefault()?.Value ?? 0);
                var amountPercent = coupon.DiscountPercent;

                if (minAmount > 0 && total < minAmount)
                {
                    throw new HttpStatusException(HttpStatusCode.BadRequest, "Not enough conditions.");
                }
                if(maxAmount > 0 && total > maxAmount)
                {
                    throw new HttpStatusException(HttpStatusCode.BadRequest, "Not enough conditions.");
                }

                var amountDiscount = total * (1 - (amountPercent / 100));
                if(amountDiscount >= maxDiscount) amountDiscount = maxDiscount;

                total -= amountDiscount;
                if (total < 0) throw new HttpStatusException(HttpStatusCode.BadRequest, "Applied coupon failure.");

                return new {
                    total,
                    amountDiscount
                };

            }
            catch (Exception ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);

            }
        }

        public async Task<bool> DeleteCouponAsync(Guid couponId, CancellationToken userCancellationToken)
        {
            try
            {
               var deleteCoupon = await _context
                                                .Coupons
                                                .Where(c => c.CouponId == couponId)
                                                .FirstOrDefaultAsync(userCancellationToken)
                                                ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Coupon not found.");

                _context.Remove(deleteCoupon);
                await _context.SaveChangesAsync(userCancellationToken);
                return true;
            }
            catch(SqlException ex)
            {
                throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
            }
        }

        public async Task<Coupon> GetCouponByIdAsync(Guid couponId, CancellationToken userCancellationToken)
        {
            try
            {
                var couponById = await _context
                                            .Coupons
                                            .Where(c => c.CouponId == couponId)
                                            .Include(c => c.CouponConditions)
                                            .ThenInclude(cc => cc.Condition)
                                            .FirstOrDefaultAsync(userCancellationToken)
                                            ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Coupon not found.");
                return couponById;
            }
            catch(Exception ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<List<Coupon>> GetListCouponAsync(string? sort, string? range, string? filter,
            HttpResponse response, CancellationToken userCancellationToken)
        {
            try
            {
                var rangeValues = Helpers.ParseString<int>(range);

                if (rangeValues.Count == 0)
                {
                    rangeValues.AddRange(new List<int> { 0, 5 });
                }

                var listCouponQuery =  _context
                                               .Coupons
                                               .Include(c => c.CouponConditions)
                                               .ThenInclude(cc => cc.Condition);
                
                var totalCoupon = await listCouponQuery.CountAsync(userCancellationToken);

                var perPage = rangeValues[1] - rangeValues[0] + 1;
                var currentPage = Convert.ToInt32(Math.Ceiling((double)rangeValues[0] / perPage)) + 1;

                //logic for filter or sort...
                var listCoupon = await listCouponQuery
                   .AsNoTracking()
                   .ToListAsync(userCancellationToken);


                listCoupon = listCoupon
                    .Skip((currentPage - 1) * perPage)
                    .Take(perPage).ToList();

                response.Headers.Append("Access-Control-Expose-Headers", "Content-Range");
                response.Headers.Append("Content-Range", $"coupons {rangeValues[0]}-{rangeValues[1]}/{totalCoupon}");

                return listCoupon;

            }
            catch (Exception ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<Coupon> PostCouponAsync(CouponDto couponDto, CancellationToken userCancellationToken)
        {
            try
            {
                if(_context.Coupons.Where(c => c.CouponCode == couponDto.CouponCode).Any())
                {
                    throw new HttpStatusException((HttpStatusCode.Conflict),"This coupon code has been exist.");
                }

                var newCoupon = new Coupon() {
                    CouponId = Guid.NewGuid(),
                    CouponCode = couponDto.CouponCode,
                    DiscountPercent = couponDto.DiscountPercent,
                    IsActive = couponDto.IsActive,
                };

                List<CouponCondition> listCouponCondition = new();
                List<Condition> listCondition = new();
                if(couponDto.OtherConditions is not null)
                {
                    foreach (var item in couponDto.OtherConditions)
                    {
                        var newCondition = new Condition()
                        {
                            ConditionId = Guid.NewGuid(),
                            Attribute = item.OtherAttribute,
                            Operator = item.OtherOperator,
                        };

                        var newCouponCondition = new CouponCondition()
                        {
                            ConditionId = newCondition.ConditionId,
                            Condition = newCondition,
                            CouponId = newCoupon.CouponId,
                            Coupon = newCoupon,
                            Value = item.OtherValue,
                        };
                        listCondition.Add(newCondition);
                        listCouponCondition.Add(newCouponCondition);
                    }
                }

                await _context.Coupons.AddAsync(newCoupon, userCancellationToken);
                await _context.Conditions.AddRangeAsync(listCondition, userCancellationToken);
                await _context.CouponConditions.AddRangeAsync(listCouponCondition, userCancellationToken);

                await _context.SaveChangesAsync(userCancellationToken);
                return newCoupon;
            }
            catch (DbUpdateException ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<Coupon> UpdateCouponAsync(Coupon couponDto, Guid couponId,CancellationToken userCancellationToken)
        {

            try
            {
                var coupons = await _context
                                             .Coupons
                                             .Include(c => c.CouponConditions)
                                             .ThenInclude(cc => cc.Condition)
                                             .ToListAsync(userCancellationToken);

                if(coupons.Any(c => c.CouponId != couponDto.CouponId && c.CouponCode == couponDto.CouponCode)) {
                    throw new HttpStatusException(HttpStatusCode.Conflict, "CouponCode was existed.");
                }

                var updateCoupon =  coupons
                                            .Where(c => c.CouponId == couponDto.CouponId)
                                            .FirstOrDefault()
                                            ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Coupon not found.");
                
                updateCoupon.IsActive = couponDto.IsActive;
                updateCoupon.DiscountPercent = couponDto.DiscountPercent;
                updateCoupon.CouponCode = couponDto.CouponCode;

                await _context.SaveChangesAsync(userCancellationToken);

                return updateCoupon;
            }
            catch (DbUpdateException ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
