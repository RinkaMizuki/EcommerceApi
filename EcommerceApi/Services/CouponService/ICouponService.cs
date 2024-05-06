using EcommerceApi.Dtos.Admin;
using EcommerceApi.Dtos.User;
using EcommerceApi.Models.Coupon;

namespace EcommerceApi.Services.CouponService
{
    public interface ICouponService
    {
        public Task<Coupon> PostCouponAsync(CouponDto couponDto, CancellationToken userCancellationToken);
        public Task<bool> DeleteCouponAsync(Guid couponId, CancellationToken userCancellationToken);
        public Task<Coupon> UpdateCouponAsync(Coupon couponDto, Guid couponId, CancellationToken userCancellationToken);
        public Task<List<Coupon>> GetListCouponAsync(string? sort, string? range, string? filter, HttpResponse response, CancellationToken userCancellationToken);
        public Task<Coupon> GetCouponByIdAsync(Guid couponId, CancellationToken userCancellationToken);
        public Task<object> ApplyCouponProductAsync(CouponProductDto couponProductDto, CancellationToken userCancellationToken);
    }
}
