namespace EcommerceApi.Dtos.Admin
{
    public class ContactDto
    {
        public int UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
