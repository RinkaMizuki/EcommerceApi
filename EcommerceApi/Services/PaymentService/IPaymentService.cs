using EcommerceApi.Dtos.User;
using EcommerceApi.Responses;

namespace EcommerceApi.Services.PaymentService
{
    public interface IPaymentService
    {
        public Task<PaymentResponse> GetPaymentOrder(PaymentDto paymentDto, HttpRequest httpRequest, CancellationToken cancellationToken);

    }
}
