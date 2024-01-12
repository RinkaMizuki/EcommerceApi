using MimeKit;

namespace EcommerceApi.Models.Message
{
    public class Message
    {
        public List<MailboxAddress> To { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public Message(List<string> to, string subject, string content)
        {
            To = new List<MailboxAddress>();
            To.AddRange(to.Select(address => new MailboxAddress(address, address)));
            Subject = subject;
            Content = content;
        }
    }
}
