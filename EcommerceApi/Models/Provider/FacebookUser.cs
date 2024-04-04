using System.Text.Json.Serialization;

namespace EcommerceApi.Models.Provider
{
    public class FacebookUser
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
        [JsonPropertyName("picture")]
        public Picture Picture { get; set; }
    }
    public class Picture
    {
        [JsonPropertyName("data")]
        public Data Data { get; set; }
    }
    public class Data
    {
        [JsonPropertyName("height")]
        public int Height { get; set; }
        [JsonPropertyName("width")]
        public int Width { get; set; }
        [JsonPropertyName("is_silhouette")]
        public bool IsSilhouette { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
    }
}
