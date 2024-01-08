using EcommerceApi.Models.Rate;

namespace EcommerceApi.Services.FeedbackService
{
    public interface IFeedbackService
    {
        public Task<List<Rate>> GetListFeedback(CancellationToken userCancellationToken);
    }
}
