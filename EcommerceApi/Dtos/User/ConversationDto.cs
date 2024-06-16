namespace EcommerceApi.Dtos.User
{
    public class ConversationDto
    {
        public Guid UserId { get; set; }
        public string? Title { get; set; } = string.Empty;
        public string? LastestMessage { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Guid AdminId { get; set; }
    }
}
