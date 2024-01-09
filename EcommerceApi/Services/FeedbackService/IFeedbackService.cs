using EcommerceApi.Dtos.Admin;
using EcommerceApi.Models.Feedback;

namespace EcommerceApi.Services.FeedbackRateService
{
    public interface IFeedbackService
    {
        public Task<FeedbackRate> PostFeedbackAsync(FeedbackDto feedbackDto, int rateId, string userName,CancellationToken userCancellationToken);
        public Task<bool> DeleteFeedbackAsync(int feedbackId, CancellationToken userCancellationToken);
        public Task<FeedbackRate> UpdateFeedbackAsync(FeedbackDto feedbackDto, string userName, int feedbackId,CancellationToken userCancellationToken);
        public Task<List<FeedbackRate>> GetListFeedbackAsync(CancellationToken userCancellationToken);
    }
}
