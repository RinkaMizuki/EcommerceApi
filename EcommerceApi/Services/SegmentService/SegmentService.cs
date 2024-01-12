using EcommerceApi.Dtos.Admin;
using EcommerceApi.ExtensionExceptions;
using EcommerceApi.Models;
using EcommerceApi.Models.Segment;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace EcommerceApi.Services.SegmentService
{
    public class SegmentService : ISegmentService
    {
        private readonly EcommerceDbContext _context;

        public SegmentService(EcommerceDbContext context)
        {
            _context = context;
        }

        public async Task<bool> DeleteSegmentAsync(int segmentId, CancellationToken userCancellationToken)
        {
            try
            {
                var deleteSegment = await _context
                                              .Segments
                                              .Where(s => s.SegmentId == segmentId)
                                              .FirstOrDefaultAsync(userCancellationToken)
                                              ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Segment not found.");

                _context.Segments.Remove(deleteSegment);

                await _context.SaveChangesAsync(userCancellationToken);

                return true;
            }
            catch (DbUpdateException ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<List<Segment>> GetListSegmentAsync(CancellationToken userCancellationToken)
        {
            try
            {
                var listSegment = await _context
                                            .Segments
                                            .Include(s => s.Users)
                                            .ThenInclude(us => us.User)
                                            //.ThenInclude(u => u.Rates)
                                            //.ThenInclude(ur => ur.FeedbackRate)
                                            //.Include(s => s.Users)
                                            //.ThenInclude(u => u.User)
                                            //.ThenInclude(u => u.Orders)
                                            .AsNoTracking()
                                            .ToListAsync(userCancellationToken);
                return listSegment;
            }
            catch (DbUpdateException ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<Segment> PostSegmentAsync(SegmentDto segmentDto, CancellationToken userCancellationToken)
        {
            try
            {
                if(_context.Segments.Where(s => s.Title == segmentDto.Title).Any())
                {
                    throw new HttpStatusException(HttpStatusCode.Conflict, "This title segment has been exist.");
                }
                var newSegment = new Segment()
                {
                    Title = segmentDto.Title,
                    CreatedAt = DateTime.Now,
                    ModifiedAt = DateTime.Now,
                };

                await _context.Segments.AddAsync(newSegment, userCancellationToken);

                await _context.SaveChangesAsync(userCancellationToken);

                return newSegment;
            }
            catch (DbUpdateException ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<Segment> UpdateSegmentAsync(SegmentDto segmentDto, int segmentId, CancellationToken userCancellationToken)
        {
            try
            {
                var updateSegment = await _context
                                            .Segments
                                            .Where(s => s.SegmentId == segmentId)
                                            .FirstOrDefaultAsync(userCancellationToken)
                                            ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Segment not found.");
                updateSegment.Title = segmentDto.Title;
                updateSegment.ModifiedAt = DateTime.Now;

                await _context.SaveChangesAsync(userCancellationToken);
                return updateSegment;
            }
            catch (DbUpdateException ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}

