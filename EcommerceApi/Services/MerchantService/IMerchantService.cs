using EcommerceApi.Dtos.User;
using EcommerceApi.Models.Payment;

namespace EcommerceApi.Services.MerchantService
{
    public interface IMerchantService
    {
        public Task<Merchant> PostMerchantAsync(MerchantDto merchantDto, CancellationToken cancellationToken);
        public Task<Merchant> UpdateMerchantAsync(MerchantDto merchantDto, Guid merchantId, CancellationToken cancellationToken);
        public Task<bool> DeleteMerchantAsync(Guid merchantId, CancellationToken cancellationToken);
        public Task<List<Merchant>> GetListMerchant(CancellationToken cancellationToken);
    }
}
