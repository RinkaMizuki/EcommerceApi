using EcommerceApi.Models.UserAddress;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApi.Models.Chat
{
    public class Participation
    {
        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
        public Guid ConversationId { get; set; }
        [ForeignKey("ConversationId")]
        public Conversation Conversation { get; set; }
        public DateTime JoinedAt { get; set; }
        public Guid AdminId { get; set; }
        public string Email { get; set; } = string.Empty;
    }
}
