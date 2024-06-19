using EcommerceApi.Models.Chat;

namespace EcommerceApi.Responses
{
    public class ConversationResponse
    {
        public Guid ConversationId { get; set; }
        public DateTime StartedAt { get; set; }
        public string? Title { get; set; } = string.Empty;
        public bool IsSeen { get; set; }
        public Message LastMessage { get; set; } = new();
    }
}
