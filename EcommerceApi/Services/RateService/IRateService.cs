using EcommerceApi.Dtos.Admin;
using EcommerceApi.Models.Rate;
using EcommerceApi.Models.Segment;

namespace EcommerceApi.Services.FeedbackService
{
    public interface IRateService
    {
        public Task<List<Rate>> GetListFeedbackAsync(CancellationToken userCancellationToken);
        public Task<Rate> UpdateFeedbackAsync(RateDto rateDto, int rateId,CancellationToken userCancellationToken);
        public Task<Boolean> DeleteFeedbackAsync(int rateId, CancellationToken userCancellationToken);
        public Task<Rate> PostFeedbackAsync(RateDto rateDto, CancellationToken userCancellationToken);
    }
}
