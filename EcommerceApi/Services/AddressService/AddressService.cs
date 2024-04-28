using EcommerceApi.Dtos.User;
using EcommerceApi.ExtensionExceptions;
using EcommerceApi.Models;
using EcommerceApi.Models.UserAddress;
using Microsoft.EntityFrameworkCore;
using Mono.TextTemplating;
using Org.BouncyCastle.Utilities.Net;
using System.Net;

namespace EcommerceApi.Services.AddressService
{
    public class AddressService : IAddressService
    {
        private readonly EcommerceDbContext _context;
        public AddressService(EcommerceDbContext context) {
            _context = context;
        }
        public async Task<bool> DeleteUserAddressAsync(Guid addressId, CancellationToken cancellationToken)
        {
            try
            {
                var deleteAddress = await _context
                                              .UserAddresses
                                              .Where(a => a.Id.Equals(addressId))
                                              .FirstOrDefaultAsync(cancellationToken)
                                              ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Address not found.");
                _context.UserAddresses.Remove(deleteAddress);
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch(Exception ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<List<UserAddress>> GetListUserAddressAsync(Guid userId, CancellationToken cancellationToken)
        {
            try
            {
                var listAddress = await _context
                                            .UserAddresses
                                            .Where(a => a.User.UserId.Equals(userId))
                                            .OrderByDescending(a => a.IsDeliveryAddress)
                                            .AsNoTracking()
                                            .ToListAsync(cancellationToken);
                return listAddress;
            }
            catch (Exception ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<UserAddress> PostUserAddressAsync(UserAddressDto userAddressDto, Guid userId, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _context
                                    .Users
                                    .Where(u => u.UserId.Equals(userId))
                                    .FirstOrDefaultAsync(cancellationToken)
                                    ?? throw new HttpStatusException(HttpStatusCode.NotFound, "User not found.");
                var newAddress = new UserAddress()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    User = user,
                    Name = userAddressDto.Name,
                    Phone = userAddressDto.Phone,
                    Country = userAddressDto.Country,
                    State = userAddressDto.State,
                    City = userAddressDto.City,
                    District = userAddressDto.District,
                    Address = userAddressDto.Address,
                    Town = userAddressDto.Town,
                    IsDeliveryAddress = userAddressDto.IsDeliveryAddress,
                    IsPickupAddress = userAddressDto.IsPickupAddress,
                    IsReturnAddress = userAddressDto.IsReturnAddress,
                };
                await _context.UserAddresses.AddAsync(newAddress, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                return newAddress;

            }
            catch (Exception ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<UserAddress> UpdateUserAddressAsync(UserAddressDto userAddressDto, Guid addressId, CancellationToken cancellationToken)
        {
            try
            {
                var updateAddress = await _context
                                                   .UserAddresses
                                                   .FirstOrDefaultAsync(a => a.Id.Equals(addressId), cancellationToken)
                                                   ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Address not found.");

                if (userAddressDto.IsDeliveryAddress)
                {
                    var changeDefaultAddress = await _context
                                                             .UserAddresses
                                                             .FirstOrDefaultAsync(ua => ua.IsDeliveryAddress == true && ua.Id != addressId, cancellationToken);
                    if (changeDefaultAddress is not null)
                    {
                        changeDefaultAddress.IsDeliveryAddress = false;
                    }
                }

                updateAddress.Name = userAddressDto.Name;
                updateAddress.Phone = userAddressDto.Phone;
                updateAddress.Country = userAddressDto.Country;
                updateAddress.State = userAddressDto.State;
                updateAddress.District = userAddressDto.District;
                updateAddress.City = userAddressDto.City;
                updateAddress.Address = userAddressDto.Address;
                updateAddress.Town = userAddressDto.Town;
                updateAddress.IsDeliveryAddress = userAddressDto.IsDeliveryAddress;
                updateAddress.IsPickupAddress = userAddressDto.IsPickupAddress;
                updateAddress.IsReturnAddress = userAddressDto.IsReturnAddress;

                await _context.SaveChangesAsync(cancellationToken);
                
                return updateAddress;
            }
            catch(Exception ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
