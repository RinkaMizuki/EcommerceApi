using EcommerceApi.Dtos.User;
using EcommerceApi.Responses;

namespace EcommerceApi.Services.PaymentService
{
    public interface IPaymentService
    {
        public Task<PaymentResponse> PostPaymentVnPayOrderAsync(PaymentDto paymentDto, HttpRequest httpRequest, CancellationToken cancellationToken);
        public Task<OrderResponse> PostPaymentVnPayReturnAsync(HttpRequest httpRequest, CancellationToken cancellationToken);

        public Task<IpnResponse> GetPaymentVnPayIpnAsync(HttpRequest httpRequest, CancellationToken cancellationToken);
        public Task<PaymentResponse> PostPaymentPayOSOrderAsync(PaymentDto paymentDto, HttpRequest httpRequest, CancellationToken cancellationToken);
        public Task<OrderResponse> PostPaymentPayOSReturnAsync(HttpRequest httpRequest, CancellationToken cancellationToken);
        public Task<OrderResponse> PostPaymentPayOsWebhookUrlAsync(HttpRequest httpRequest, CancellationToken cancellationToken);

    }
}
