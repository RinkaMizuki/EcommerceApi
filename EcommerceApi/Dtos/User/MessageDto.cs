namespace EcommerceApi.Dtos.User
{
    public class MessageDto
    {
        public Guid? MessageId { get; set; }
        public Guid SenderId { get; set; }
        public Guid ConversationId { get; set; }
        public string MessageContent { get; set; } = string.Empty;
        public Guid? OriginalMessageId { get; set; } = null;
    }
}
