using EcommerceApi.Dtos.Admin;
using EcommerceApi.ExtensionExceptions;
using EcommerceApi.Models;
using EcommerceApi.Models.Feedback;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Net;

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
            try
            {
                var rate = await _context
                                    .Rates
                                    .Where(r => r.RateId == rateId)
                                    .Include(r => r.FeedbackRate)
                                    .FirstOrDefaultAsync(userCancellationToken)
                                    ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Rate not found.");

                if (rate.FeedbackRate is not null) throw new HttpStatusException(HttpStatusCode.BadRequest, "This rate has been feedbacked");

                //if(_context.Entry(rate).State == EntityState.Detached)
                //{
                //    _context.Rates.Attach(rate);
                //}

                var newFeedback = new FeedbackRate()
                {
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
            catch(SqlException ex)
            {
                throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
            }
        }

        public async Task<bool> DeleteFeedbackAsync(int feedbackId, CancellationToken userCancellationToken)
        {
            try
            {
                var feedback = await _context
                                        .Feedbacks
                                        .Where(fb => fb.FeedbackRateId == feedbackId)
                                        .FirstOrDefaultAsync(userCancellationToken)
                                        ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Feedback not found.");

                //việc attached entity vào context thì entity sẽ ở trạng thái unchanged 

                if (_context.Entry(feedback).State == EntityState.Detached)
                {
                    _context.Feedbacks.Attach(feedback);
                }

                _context.Entry(feedback).State = EntityState.Deleted;

                await _context.SaveChangesAsync(userCancellationToken);
                
                return true;
            }
            catch(SqlException ex)
            {
                throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
            }
        }

        public async Task<List<FeedbackRate>> GetListFeedbackAsync(CancellationToken userCancellationToken)
        {
            try
            {
                var listFeedback = await _context
                                             .Feedbacks
                                             .ToListAsync(userCancellationToken);
                return listFeedback;
            }
            catch (SqlException ex)
            {
                throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
            }
            
        }

        //Fix: System.NotSupportedException: Serialization and deserialization of 'System.Action'
        // ==> Missing keyword await 
        public async Task<FeedbackRate> UpdateFeedbackAsync(FeedbackDto feedbackDto, string userName, int feedbackId, CancellationToken userCancellationToken)
        {
            try
            {
                var updateFeedback = await _context
                                                .Feedbacks
                                                .Where(fb => fb.FeedbackRateId == feedbackId)
                                                .FirstOrDefaultAsync(userCancellationToken)
                                                ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Feedback not found.");

                if (_context.Entry(updateFeedback).State == EntityState.Detached)
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
            catch(SqlException ex)
            {
                throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
            }
        }
    }
}
