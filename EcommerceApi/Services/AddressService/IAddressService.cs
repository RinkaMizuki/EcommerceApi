using EcommerceApi.Dtos.User;
using EcommerceApi.Models.UserAddress;

namespace EcommerceApi.Services.AddressService
{
    public interface IAddressService
    {
        public Task<UserAddress> PostUserAddressAsync(UserAddressDto userAddressDto, int userId, CancellationToken cancellationToken);
        public Task<bool> DeleteUserAddressAsync(Guid addressId, CancellationToken cancellationToken);
        public Task<UserAddress> UpdateUserAddressAsync(UserAddressDto userAddressDto, Guid addressId, CancellationToken cancellationToken);
        public Task<List<UserAddress>> GetListUserAddressAsync(int userId, CancellationToken cancellationToken);
    }
}
