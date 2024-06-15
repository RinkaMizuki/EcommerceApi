using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.Models.Chat
{
    public class Conversation
    {
        [Key]
        public Guid ConversationId { get; set; }
        public DateTime StartedAt { get; set; }
        public string? Title { get; set; } = string.Empty;
        public string? LastestMessage { get; set; } = string.Empty;
        public DateTime? LastestSend { get; set; }
        public List<Message> Messages { get; set; } = new();
    }
}
