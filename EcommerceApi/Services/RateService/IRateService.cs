using EcommerceApi.Dtos.Admin;
using EcommerceApi.Models.Rate;
using EcommerceApi.Responses;

namespace EcommerceApi.Services.FeedbackService
{
    public interface IRateService
    {
        public Task<List<RateResponse>> GetListRateAsync(string sort, string range, string filter, HttpResponse response,
            CancellationToken userCancellationToken);
        public Task<Rate> GetRateByIdAsync(int rateId, CancellationToken userCancellationToken);
        public Task<Rate> UpdateRateAsync(RateDto rateDto, int rateId, CancellationToken userCancellationToken);
        public Task<Boolean> DeleteRateAsync(int rateId, CancellationToken userCancellationToken);
        public Task<Rate> PostRateAsync(RateDto rateDto, CancellationToken userCancellationToken);
    }
}