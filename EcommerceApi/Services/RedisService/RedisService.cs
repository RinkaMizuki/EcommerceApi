using EcommerceApi.ExtensionExceptions;
using StackExchange.Redis;
using System.Net;

namespace EcommerceApi.Services.RedisService
{
    public class RedisService : IRedisService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<RedisService> _logger;
        public RedisService(IConnectionMultiplexer redis, ILogger<RedisService> logger)
        {
            _redis = redis;
            _logger = logger;

        }
        public async Task<string> GetValueAsync(string key)
        {
            try
            {
                var db = _redis.GetDatabase();
                var value = await db.StringGetAsync(key);
                return string.IsNullOrWhiteSpace(value) ? default : value;

            }
            catch (RedisConnectionException ex)
            {
                _logger.LogError(ex, "Redis connection error while getting value for key {Key}", key);
                throw new Exception("Redis connection error");
            }
            catch (Exception ex) {
                _logger.LogError(ex, "An error occurred while getting value for key {Key}", key);
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<bool> RemoveValueAsync(string key)
        {
            try
            {
                var db = _redis.GetDatabase();
                bool wasRemoved = await db.KeyDeleteAsync(key);
                return wasRemoved;
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogError(ex, "Redis connection error while setting value for key {Key}", key);
                throw new Exception("Redis connection error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while setting value for key {Key}", key);
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
            catch (RedisConnectionException ex)
            {
                _logger.LogError(ex, "Redis connection error while setting value for key {Key}", keyValue.Key);
                throw new Exception("Redis connection error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while setting value for key {Key}", keyValue.Key);
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
