using EcommerceApi.Dtos.Admin;
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

        public async Task<bool> DeleteCouponAsync(Guid couponId, CancellationToken userCancellationToken)
        {
            try
            {
               var deleteCoupon = await _context
                                                .Coupons
                                                .Where(c => c.CouponId == couponId)
                                                .FirstOrDefaultAsync(userCancellationToken)
                                                ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Coupon not found.");
                await _context.SaveChangesAsync(userCancellationToken);
                return true;
            }
            catch(SqlException ex)
            {
                throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
            }
        }

        public async Task<List<Coupon>> GetListCouponAsync(CancellationToken userCancellationToken)
        {
            try
            {
                var listCoupon = await _context
                                               .Coupons
                                               .Include(c => c.CouponConditions)
                                               .ThenInclude(cc => cc.Condition)
                                               .AsNoTracking()
                                               .ToListAsync(userCancellationToken);
                return listCoupon;
            }
            catch (SqlException ex)
            {
                throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
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

                var newCondition = new Condition() {
                    ConditionId = Guid.NewGuid(),
                    Attribute = couponDto.Attribute,
                    Operator = couponDto.Operator,
                };

                var newCouponCondition = new CouponCondition()
                {
                    ConditionId = newCondition.ConditionId,
                    Condition = newCondition,
                    CouponId = newCoupon.CouponId,
                    Coupon = newCoupon,
                    Value = couponDto.Value,
                };
              
                var otherCondition = new Condition()
                {
                    ConditionId = Guid.NewGuid(),
                    Attribute = couponDto.OtherCondition.OtherAttribute,
                    Operator = couponDto.OtherCondition.OtherOperator,
                };

                var otherCouponCondition = new CouponCondition()
                {
                    ConditionId = otherCondition.ConditionId,
                    Condition = otherCondition,
                    CouponId = newCoupon.CouponId,
                    Coupon = newCoupon,
                    Value = couponDto.OtherCondition.OtherValue,
                };

                listCondition.Add(newCondition);
                listCondition.Add(otherCondition);
                listCouponCondition.Add(newCouponCondition);
                listCouponCondition.Add(otherCouponCondition);

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

        public async Task<Coupon> UpdateCouponAsync(bool isActive, Guid couponId,CancellationToken userCancellationToken)
        {

            try
            {
                var updateCoupon = await _context
                                             .Coupons
                                             .Where(c => c.CouponId == couponId)
                                             .Include(c => c.CouponConditions)
                                             .ThenInclude(cc => cc.Condition)
                                             .FirstOrDefaultAsync(userCancellationToken)
                                             ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Coupon not found.");
                updateCoupon.IsActive = isActive;

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
