using EcommerceApi.Dtos.User;
using EcommerceApi.Responses;

namespace EcommerceApi.Services.PaymentService
{
    public interface IPaymentService
    {
        public Task<PaymentResponse> PostPaymentOrderAsync(PaymentDto paymentDto, HttpRequest httpRequest, CancellationToken cancellationToken);
        public Task<InvoiceResponse> PostPaymentReturnAsync(HttpRequest httpRequest, CancellationToken cancellationToken);

        public Task<IpnResponse> GetPaymentIpnAsync(HttpRequest httpRequest, CancellationToken cancellationToken);
    }
}
