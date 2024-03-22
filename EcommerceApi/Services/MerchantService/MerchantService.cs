using EcommerceApi.Config;
using EcommerceApi.Constant;
using EcommerceApi.Dtos.User;
using EcommerceApi.ExtensionExceptions;
using EcommerceApi.Models;
using EcommerceApi.Models.Payment;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;

namespace EcommerceApi.Services.MerchantService
{
    public class MerchantService : IMerchantService
    {
        private readonly VnPayConfig _config;
        private readonly IConfiguration _configSetting;
        private readonly EcommerceDbContext _context;
        public MerchantService(EcommerceDbContext context, IOptions<VnPayConfig> options, IConfiguration configSetting)
        {
            _config = options.Value;
            _configSetting = configSetting;
            _context = context;
        }
        public async Task<Merchant> PostMerchantAsync(MerchantDto merchantDto, CancellationToken cancellationToken)
        {
            try
            {
                var newMerchant = new Merchant()
                {
                    MerchantId = Guid.NewGuid(),
                    MerchantName = merchantDto.MerchantName,
                    MerchantIpnUrl = merchantDto.MerchantIpnUrl,
                    MerchantRetrunUrl = merchantDto.MerchantRetrunUrl,
                    MerchantWebUrl = merchantDto.MerchantWebUrl,
                };
                if (newMerchant.MerchantName.ToLower() == MerchantKey.Ecommerce)
                {
                    newMerchant.SecretKey = _configSetting.GetSection("MerchantConfiguration:MerchantEcommerce").Value!;
                }
                await _context.Merchants.AddAsync(newMerchant, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                return newMerchant;
            }
            catch (Exception ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<Merchant> UpdateMerchantAsync(MerchantDto merchantDto, Guid merchantId, CancellationToken cancellationToken)
        {
            var updateMerchant = await _context
                                               .Merchants
                                               .Where(m => m.MerchantId == merchantId)
                                               .FirstOrDefaultAsync(cancellationToken)
                                               ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Merchant not found.");
            try
            {
                updateMerchant.IsActive = merchantDto.IsActive;
                await _context.SaveChangesAsync(cancellationToken);
                return updateMerchant;
            }
            catch (Exception ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        public async Task<bool> DeleteMerchantAsync(Guid merchantId, CancellationToken cancellationToken)
        {
            var deleteMerchant = await _context
                                               .Merchants
                                               .Where(m => m.MerchantId == merchantId)
                                               .FirstOrDefaultAsync(cancellationToken)
                                               ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Merchant not found.");
            try
            {
                _context.Merchants.Remove(deleteMerchant);
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<List<Merchant>> GetListMerchant(CancellationToken cancellationToken)
        {
            try
            {
                var listMerchant = await _context
                                 .Merchants
                                 .AsNoTracking()
                                 .ToListAsync(cancellationToken);
                return listMerchant;
            }
            catch (Exception ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
