namespace EcommerceApi.Config;

public class CloudflareR2Config
{
    public string bucketName { get; set; } = string.Empty;
    public string accountId { get; set; } = string.Empty;
    public string accessKey { get; set; } = string.Empty;
    public string accessSecret { get; set; } = string.Empty;
    public string publicUrl {  get; set; } = string.Empty;
}