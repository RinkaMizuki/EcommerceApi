using MimeKit;

namespace EcommerceApi.Models.Message
{
    public class Message
    {
        public MailboxAddress To { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public Message(string to, string name, string subject, string content)
        {
            To = new MailboxAddress(name, to);
            Subject = subject;
            Content = content;
        }
    }
}
