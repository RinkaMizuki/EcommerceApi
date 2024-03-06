namespace EcommerceApi.Responses
{
    public class UserProfileResponse
    {
        public string UserName { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public string? Phone { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}
