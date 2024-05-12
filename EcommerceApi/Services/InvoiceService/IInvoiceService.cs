using EcommerceApi.Responses;

namespace EcommerceApi.Services.InvoiceService
{
    public interface IInvoiceService
    {
        public Task<List<InvoiceResponse>> GetListInvoiceAsync(string filter, string range, string sort, HttpResponse response, CancellationToken cancellationToken); 
    }
}
