using StackExchange.Redis;

namespace BlogApp.Infrastructure.Services;

public class RedisCacheService(IDistributedCache cache, ILogger<RedisCacheService> logger, IConfiguration configuration) : ICacheService
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var cachedValue = await cache.GetStringAsync(key);
            if (string.IsNullOrEmpty(cachedValue))
            {
                logger.LogDebug("Cache miss for key: {Key}", key);
                return default;
            }

            logger.LogDebug("Cache hit for key: {Key}", key);
            return JsonSerializer.Deserialize<T>(cachedValue, _jsonOptions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting value from cache for key: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
            var options = new DistributedCacheEntryOptions();

            if (expiration.HasValue) options.SetAbsoluteExpiration(expiration.Value);

            await cache.SetStringAsync(key, serializedValue, options);
            logger.LogDebug("Cached value for key: {Key} with expiration: {Expiration}", key, expiration);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error setting value in cache for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await cache.RemoveAsync(key);
            logger.LogDebug("Removed cache entry for key: {Key}", key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing cache entry for key: {Key}", key);
        }
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        try
        {
            logger.LogInformation("Pattern removal requested for pattern: {Pattern}", pattern);

            // Redis server bağlantısını al
            var multiplexer = await ConnectionMultiplexer.ConnectAsync(configuration["RedisConnection"]!);

            var endpoints = multiplexer.GetEndPoints();
            foreach (var endpoint in endpoints)
            {
                var server = multiplexer.GetServer(endpoint);
                if (!server.IsConnected)
                    continue;

                // SCAN ile pattern'e uyan tüm key'leri bul
                var keys = server.Keys(pattern: pattern);
                foreach (var key in keys)
                {
#pragma warning disable CS8604 // Possible null reference argument.
                    await cache.RemoveAsync(key);
#pragma warning restore CS8604 // Possible null reference argument.
                    logger.LogDebug("Removed cache entry for key: {Key}", key);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing cache entries for pattern: {Pattern}", pattern);
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            var value = await cache.GetStringAsync(key);
            return !string.IsNullOrEmpty(value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking existence for key: {Key}", key);
            return false;
        }
    }

    public async Task<long> IncrementAsync(string key, long value = 1)
    {
        try
        {
            // Note: This is a simplified implementation
            // In a real Redis implementation, you would use INCRBY command
            var currentValue = await GetAsync<long?>(key) ?? 0L;
            var newValue = currentValue + value;
            await SetAsync(key, newValue);
            return newValue;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error incrementing value for key: {Key}", key);
            return 0L;
        }
    }

    public async Task<TimeSpan?> GetTimeToLiveAsync(string key)
    {
        try
        {
            logger.LogDebug("TTL check requested for key: {Key}", key);

            // Redis bağlantısını al
            var multiplexer = await ConnectionMultiplexer.ConnectAsync(configuration["RedisConnection"]!);

            var db = multiplexer.GetDatabase();
            var ttl = await db.KeyTimeToLiveAsync(key);

            if (ttl.HasValue)
                logger.LogDebug("TTL for key {Key} is {TTL}", key, ttl.Value);
            else
                logger.LogDebug("No TTL found for key {Key} (may not exist or has no expiration)", key);

            return ttl;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting TTL for key: {Key}", key);
            return null;
        }
    }
}