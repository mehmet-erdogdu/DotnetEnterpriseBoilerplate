using StackExchange.Redis;

namespace BlogApp.Infrastructure.Services;

// Interface to wrap IDistributedCache methods for easier testing
public interface IDistributedCacheWrapper
{
    Task<string?> GetStringAsync(string key, CancellationToken token = default);
    Task SetStringAsync(string key, string value, DistributedCacheEntryOptions options, CancellationToken token = default);
    Task RemoveAsync(string key, CancellationToken token = default);
}

// Implementation that wraps the real IDistributedCache
public class DistributedCacheWrapper : IDistributedCacheWrapper
{
    private readonly IDistributedCache _cache;

    public DistributedCacheWrapper(IDistributedCache cache)
    {
        _cache = cache;
    }

    public Task<string?> GetStringAsync(string key, CancellationToken token = default)
    {
        return _cache.GetStringAsync(key, token);
    }

    public Task SetStringAsync(string key, string value, DistributedCacheEntryOptions options, CancellationToken token = default)
    {
        return _cache.SetStringAsync(key, value, options, token);
    }

    public Task RemoveAsync(string key, CancellationToken token = default)
    {
        return _cache.RemoveAsync(key, token);
    }
}

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCacheWrapper _cache;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly IConfiguration _configuration;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public RedisCacheService(IDistributedCache cache, ILogger<RedisCacheService> logger, IConfiguration configuration)
    {
        _cache = new DistributedCacheWrapper(cache);
        _logger = logger;
        _configuration = configuration;
    }

    // Constructor for testing with mocked wrapper
    internal RedisCacheService(IDistributedCacheWrapper cacheWrapper, ILogger<RedisCacheService> logger, IConfiguration configuration)
    {
        _cache = cacheWrapper;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var cachedValue = await _cache.GetStringAsync(key);
            if (string.IsNullOrEmpty(cachedValue))
            {
                _logger.LogDebug("Cache miss for key: {Key}", key);
                return default;
            }

            _logger.LogDebug("Cache hit for key: {Key}", key);
            return JsonSerializer.Deserialize<T>(cachedValue, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting value from cache for key: {Key}", key);
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

            await _cache.SetStringAsync(key, serializedValue, options, default);
            _logger.LogDebug("Cached value for key: {Key} with expiration: {Expiration}", key, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value in cache for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _cache.RemoveAsync(key);
            _logger.LogDebug("Removed cache entry for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache entry for key: {Key}", key);
        }
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        try
        {
            _logger.LogInformation("Pattern removal requested for pattern: {Pattern}", pattern);

            // Redis server bağlantısını al
            var multiplexer = await ConnectionMultiplexer.ConnectAsync(_configuration["RedisConnection"]!);

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
                    await _cache.RemoveAsync(key);
#pragma warning restore CS8604 // Possible null reference argument.
                    _logger.LogDebug("Removed cache entry for key: {Key}", key);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache entries for pattern: {Pattern}", pattern);
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            var value = await _cache.GetStringAsync(key);
            return !string.IsNullOrEmpty(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence for key: {Key}", key);
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
            _logger.LogError(ex, "Error incrementing value for key: {Key}", key);
            return 0L;
        }
    }

    public async Task<TimeSpan?> GetTimeToLiveAsync(string key)
    {
        try
        {
            _logger.LogDebug("TTL check requested for key: {Key}", key);

            // Redis bağlantısını al
            var multiplexer = await ConnectionMultiplexer.ConnectAsync(_configuration["RedisConnection"]!);

            var db = multiplexer.GetDatabase();
            var ttl = await db.KeyTimeToLiveAsync(key);

            if (ttl.HasValue)
                _logger.LogDebug("TTL for key {Key} is {TTL}", key, ttl.Value);
            else
                _logger.LogDebug("No TTL found for key {Key} (may not exist or has no expiration)", key);

            return ttl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting TTL for key: {Key}", key);
            return null;
        }
    }
}