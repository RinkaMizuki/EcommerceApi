using EcommerceApi.Dtos.Admin;
using EcommerceApi.Models.Segment;
using EcommerceApi.Responses;

namespace EcommerceApi.Services.SegmentService
{
    public interface ISegmentService
    {
        public Task<Segment> PostSegmentAsync(SegmentDto segmentDto, CancellationToken userCancellationToken);
        public Task<bool> DeleteSegmentAsync(int segmentId, CancellationToken userCancellationToken);
        public Task<Segment> UpdateSegmentAsync(SegmentDto segmentDto, int segmentId, CancellationToken userCancellationToken);
        public Task<List<SegmentResponse>> GetListSegmentAsync(CancellationToken userCancellationToken);
    }
}
