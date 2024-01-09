using EcommerceApi.Dtos.Admin;
using EcommerceApi.Models;
using EcommerceApi.Models.Rate;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApi.Services.FeedbackService
{
    public class RateService : IRateService
    {
        private readonly EcommerceDbContext _context;
        public RateService(EcommerceDbContext context) {
            _context = context;
        }
        public async Task<bool> DeleteFeedbackAsync(int rateId, CancellationToken userCancellationToken)
        {
           var rateByID = await _context
                                        .Rates
                                        .Where(r => r.RateId == rateId)
                                        .FirstOrDefaultAsync(userCancellationToken);
            if (rateByID is null) return false;
            _context.Rates .Remove(rateByID);
            await _context.SaveChangesAsync(userCancellationToken);
            return true;
        }

        public async Task<List<Rate>> GetListFeedbackAsync(CancellationToken userCancellationToken)
        {
            var listRates = await _context
                                        .Rates
                                        .AsNoTracking()
                                        .ToListAsync(userCancellationToken);
            return listRates;
        }

        public async Task<Rate> PostFeedbackAsync(RateDto rateDto, CancellationToken userCancellationToken)
        {
            var productRate = await _context
                                            .Products
                                            .Where(p => p.ProductId == rateDto.ProductId)
                                            .Include(p => p.ProductRates)
                                            .FirstOrDefaultAsync(userCancellationToken);
            var userRate = await _context
                                        .Users.Where(u => u.UserId == rateDto.UserId)
                                        .FirstOrDefaultAsync(userCancellationToken);
          
            if (productRate is null || userRate is null) return null;
            
            if (productRate.ProductRates.Where(pr => pr.UserId == userRate.UserId).Any())
            {
                return null;
            }

            var newRate = new Rate() {
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

            await _context.Rates.AddAsync(newRate, userCancellationToken);

            await _context.SaveChangesAsync(userCancellationToken);
            return newRate;
        }

        public async Task<Rate> UpdateFeedbackAsync(RateDto rateDto, int rateId, CancellationToken userCancellationToken)
        {
            var updateRate = await _context
                                        .Rates
                                        .Where(r => r.RateId == rateId)
                                        .FirstOrDefaultAsync(userCancellationToken);

            if (updateRate is null) return null;

            updateRate.Content = rateDto.Content;
            updateRate.Star = rateDto.Star;
            updateRate.ModifiedAt = DateTime.Now;
            updateRate.Status = rateDto.Status;

            await _context.SaveChangesAsync(userCancellationToken);

            return updateRate;
        }
    }
}
