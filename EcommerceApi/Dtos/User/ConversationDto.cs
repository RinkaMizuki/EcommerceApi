namespace EcommerceApi.Dtos.User
{
    public class ConversationDto
    {
        public Guid UserId { get; set; }
        public string? Title { get; set; } = string.Empty;
        public Guid AdminId { get; set; }
    }
}
