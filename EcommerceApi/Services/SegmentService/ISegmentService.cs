using EcommerceApi.Dtos.Admin;
using EcommerceApi.Models.Segment;

namespace EcommerceApi.Services.SegmentService
{
    public interface ISegmentService
    {
        public Task<Segment> PostSegmentAsync(SegmentDto segmentDto, CancellationToken userCancellationToken);
        public Task<bool> DeleteSegmentAsync(int segmentId, CancellationToken userCancellationToken);
        public Task<Segment> UpdateSegmentAsync(SegmentDto segmentDto, int segmentId, CancellationToken userCancellationToken);
        public Task<List<Segment>> GetListSegmentAsync(CancellationToken userCancellationToken);
    }
}
