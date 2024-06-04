using EcommerceApi.ExtensionExceptions;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Net;

namespace EcommerceApi.Services.RedisService
{
    public class RedisService : IRedisService
    {
        private readonly IConnectionMultiplexer _redis;
        public RedisService(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }
        public async Task<string> GetValueAsync(string key)
        {
            try
            {
                var db = _redis.GetDatabase();
                var value = await db.StringGetAsync(key);
                return string.IsNullOrWhiteSpace(value) ? default : value;

            }
            catch (Exception ex) {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<bool> SetValueAsync(KeyValuePair<string, string> keyValue)
        {
            try
            {
                var db = _redis.GetDatabase();
                var isSuccess = await db.StringSetAsync(keyValue.Key, keyValue.Value);
                return isSuccess;
            }
            catch(Exception ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
