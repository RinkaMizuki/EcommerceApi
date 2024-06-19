using EcommerceApi.Models.UserAddress;

namespace EcommerceApi.Responses
{
    public class ParticipationResponse
    {
        public Guid UserId { get; set; }
        public User User { get; set; }
        public Guid ConversationId { get; set; }
        public ConversationResponse Conversation { get; set; }
        public DateTime JoinedAt { get; set; }
        public Guid AdminId { get; set; }
        public string Email { get; set; } = string.Empty;
    }
}
