using EcommerceApi.Models.Message;

namespace EcommerceApi.Services.MailService
{
    public interface IMailService
    {
        public Task<bool> SendEmailAsync(Message message, CancellationToken userCancellationToken);
    }
}
