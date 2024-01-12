using EcommerceApi.Config;
using EcommerceApi.ExtensionExceptions;
using EcommerceApi.Models.Message;
using EcommerceApi.Services.MailService;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Net;

namespace EcommerceApi.Services.EmailService
{
    public class MailService : IMailService
    {
        private readonly EmailConfig _emailConfig;
        public MailService(IOptions<EmailConfig> options) {
            _emailConfig = options.Value;
        }
        public async Task<bool> SendEmailAsync(Message message, CancellationToken userCancellationToken)
        {
            var emailMessage = CreateEmailMessage(message);
            await Send(emailMessage, userCancellationToken);
            return true;

        }
        private MimeMessage CreateEmailMessage(Message message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(_emailConfig.UserName, _emailConfig.From));
            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.Subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = message.Content};
            return emailMessage;
        }
        private async Task Send(MimeMessage mailMessage, CancellationToken userCancellationToken)
        {
            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.Port, true, userCancellationToken);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                await client.AuthenticateAsync(_emailConfig.UserName, _emailConfig.Password, userCancellationToken);
                await client.SendAsync(mailMessage, userCancellationToken);
            }
            catch (Exception ex)
            {
                throw new HttpStatusException(HttpStatusCode.BadRequest, ex.Message);
            }
            finally
            {
                await client.DisconnectAsync(true, userCancellationToken);
                client.Dispose();
            }
        }
    }
}
