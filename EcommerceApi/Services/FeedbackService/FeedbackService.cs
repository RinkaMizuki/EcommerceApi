using EcommerceApi.Dtos.Admin;
using EcommerceApi.Models;
using EcommerceApi.Models.Feedback;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApi.Services.FeedbackRateService
{
    public class FeedbackService : IFeedbackService
    {
        private readonly EcommerceDbContext _context;
        public FeedbackService(EcommerceDbContext context) {
            _context = context;
        }
        public async Task<FeedbackRate> PostFeedbackAsync(FeedbackDto feedbackDto,int rateId ,string userName,CancellationToken userCancellationToken)
        {
            var rate = await _context
                                    .Rates
                                    .Where(r => r.RateId == rateId)
                                    .Include(r => r.FeedbackRate)
                                    .FirstOrDefaultAsync(userCancellationToken);
            
            if (rate is null || rate.FeedbackRate is not null) return null;

            //if(_context.Entry(rate).State == EntityState.Detached)
            //{
            //    _context.Rates.Attach(rate);
            //}

            var newFeedback = new FeedbackRate() { 
                Content = feedbackDto.Content,
                CreatedAt = DateTime.Now,
                ModifiedAt = DateTime.Now,
                CreatedBy = userName,
                ModifiedBy = userName,
                Rate = rate,
                FeedbackRateId = rate.RateId,
            };
            
            //rate.FeedbackRate = newFeedback;
            
            await _context
                          .Feedbacks
                          .AddAsync(newFeedback, userCancellationToken);
            await _context.SaveChangesAsync(userCancellationToken);
            return newFeedback;
        }

        public async Task<bool> DeleteFeedbackAsync(int feedbackId, CancellationToken userCancellationToken)
        {
           var feedback = await _context
                                        .Feedbacks
                                        .Where(fb => fb.FeedbackRateId == feedbackId)
                                        .FirstOrDefaultAsync(userCancellationToken);
            if (feedback is null) return false;
            
            //việc attached entity vào context thì entity sẽ ở trạng thái unchanged 

            if(_context.Entry(feedback).State == EntityState.Detached)
            {
                _context.Feedbacks.Attach(feedback);
            }

            _context.Entry(feedback).State = EntityState.Deleted;
            await _context.SaveChangesAsync(userCancellationToken);
            return true;
        }

        public async Task<List<FeedbackRate>> GetListFeedbackAsync(CancellationToken userCancellationToken)
        {
            var listFeedback = await _context
                                            .Feedbacks
                                            .ToListAsync(userCancellationToken);
            return listFeedback;
        }

        //Fix: System.NotSupportedException: Serialization and deserialization of 'System.Action'
        // ==> Missing keyword await 
        public async Task<FeedbackRate> UpdateFeedbackAsync(FeedbackDto feedbackDto, string userName, int feedbackId, CancellationToken userCancellationToken)
        {
            var updateFeedback = await _context
                                                .Feedbacks
                                                .Where(fb => fb.FeedbackRateId == feedbackId)
                                                .FirstOrDefaultAsync(userCancellationToken);
            if (updateFeedback is null) return null;
            
            if(_context.Entry(updateFeedback).State == EntityState.Detached)
            {
                _context.Feedbacks.Attach(updateFeedback);
            }

            _context.Entry(updateFeedback).State = EntityState.Unchanged;

            updateFeedback.Content = feedbackDto.Content;
            updateFeedback.ModifiedAt = DateTime.Now;
            updateFeedback.ModifiedBy = userName;

            await _context.SaveChangesAsync(userCancellationToken);

            return updateFeedback;
        }
    }
}
