namespace EcommerceApi.Dtos.Admin
{
    public class ContactDto
    {
        public Guid UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Support { get; set; } = string.Empty;
        public string? Title { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
