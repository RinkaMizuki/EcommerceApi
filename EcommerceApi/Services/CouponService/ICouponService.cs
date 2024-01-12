using EcommerceApi.Dtos.Admin;
using EcommerceApi.Models.Coupon;

namespace EcommerceApi.Services.CouponService
{
    public interface ICouponService
    {
        public Task<Coupon> PostCouponAsync(CouponDto couponDto, CancellationToken userCancellationToken);
        public Task<bool> DeleteCouponAsync(Guid couponId, CancellationToken userCancellationToken);
        public Task<Coupon> UpdateCouponAsync(bool isActive, Guid couponId, CancellationToken userCancellationToken);
        public Task<List<Coupon>> GetListCouponAsync(CancellationToken userCancellationToken);
    }
}
