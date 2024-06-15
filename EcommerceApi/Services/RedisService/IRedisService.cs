namespace EcommerceApi.Services.RedisService
{
    public interface IRedisService
    {
        public Task<string> GetValueAsync(string key);
        public Task<bool> SetValueAsync(KeyValuePair<string, string> keyValue);
        public Task<bool> RemoveValueAsync(string key);
    }
}
