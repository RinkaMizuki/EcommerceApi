namespace EcommerceApi.Responses
{
    public class RateResponse
    {
        public int Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public int Star { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public ProductRateResponse ProductRateResponse {  get; set; }
        public string UserName { get; set; } = string.Empty;
    }
    public class ProductRateResponse
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}
