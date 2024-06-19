using EcommerceApi.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EcommerceApi.Models.Chat
{
    public class Conversation
    {
        [Key]
        public Guid ConversationId { get; set; }
        public DateTime StartedAt { get; set; }
        public string? Title { get; set; } = string.Empty;
        public bool IsSeen { get; set; }
        public List<Message> Messages { get; set; } = new();
    }
}
