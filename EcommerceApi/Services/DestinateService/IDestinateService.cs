using EcommerceApi.Dtos.Admin;
using EcommerceApi.Models.Payment;

namespace EcommerceApi.Services.DestinateService
{
    public interface IDestinateService
    {
        public Task<PaymentDestination> PostDestinationAsync(DestinationDto destinationDto, CancellationToken cancellationToken);
        public Task<List<PaymentDestination>> GetListDestinationAsync(CancellationToken cancellationToken);
        public Task<PaymentDestination> UpdateDestinationAsync(DestinationDto destinationDto, Guid destinationId,CancellationToken cancellationToken);

        public Task<bool> DeleteDestinationAsync(Guid destinationId, CancellationToken cancellationToken);

    }
}
