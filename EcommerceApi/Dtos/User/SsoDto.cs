namespace EcommerceApi.Dtos.User
{
    public class SsoDto
    {
        public int statusCode { get; set; }
        public string message { get; set; } = string.Empty;
        public string token { get; set; } = string.Empty;
    }
}
