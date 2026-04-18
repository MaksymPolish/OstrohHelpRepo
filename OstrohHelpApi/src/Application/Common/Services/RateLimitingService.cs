using System.Collections.Concurrent;
using Domain.Common;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Application.Common.Services;

/// <summary>
/// Token bucket rate limiting service
/// Each user-endpoint pair has a token bucket that refills over time
/// </summary>
public class RateLimitingService : IRateLimitingService
{
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;

    // Store last reset times for each bucket to detect window expiration
    private readonly ConcurrentDictionary<string, DateTime> _bucketResetTimes = new();

    public RateLimitingService(IConfiguration configuration, IMemoryCache cache)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public bool CanMakeRequest(Guid userId, string endpoint)
    {
        if (!IsRateLimitingEnabled())
            return true;

        var config = GetRateLimitConfig(endpoint);
        if (config == null)
            return true; // No rate limit configured for this endpoint

        var bucketKey = GetBucketKey(userId, endpoint);
        var remaining = GetRemainingRequests(userId, endpoint);

        if (remaining > 0)
        {
            // Decrement tokens
            var currentTokens = remaining - 1;
            _cache.Set(bucketKey, currentTokens, TimeSpan.FromSeconds(config.WindowSeconds));
            return true;
        }

        return false;
    }

    public int GetRemainingRequests(Guid userId, string endpoint)
    {
        if (!IsRateLimitingEnabled())
            return int.MaxValue;

        var config = GetRateLimitConfig(endpoint);
        if (config == null)
            return int.MaxValue;

        var bucketKey = GetBucketKey(userId, endpoint);
        var resetTimeKey = GetResetTimeKey(userId, endpoint);

        // Check if we need to reset the bucket (window expired)
        if (_bucketResetTimes.TryGetValue(resetTimeKey, out var lastReset))
        {
            if (DateTime.UtcNow - lastReset >= TimeSpan.FromSeconds(config.WindowSeconds))
            {
                // Window expired, reset the bucket
                _cache.Remove(bucketKey);
                _bucketResetTimes.TryUpdate(resetTimeKey, DateTime.UtcNow, lastReset);
            }
        }
        else
        {
            // First request in this window
            _bucketResetTimes.TryAdd(resetTimeKey, DateTime.UtcNow);
        }

        // Get current token count (default to max if first request)
        if (!_cache.TryGetValue(bucketKey, out int currentTokens))
        {
            currentTokens = config.MaxRequests;
            _cache.Set(bucketKey, currentTokens, TimeSpan.FromSeconds(config.WindowSeconds));
        }

        return currentTokens;
    }

    public void ResetUserLimits(Guid userId)
    {
        if (!IsRateLimitingEnabled())
            return;

        var endpoints = GetConfiguredEndpoints();
        foreach (var endpoint in endpoints)
        {
            var bucketKey = GetBucketKey(userId, endpoint);
            var resetTimeKey = GetResetTimeKey(userId, endpoint);

            _cache.Remove(bucketKey);
            _bucketResetTimes.TryRemove(resetTimeKey, out _);
        }
    }

    public int GetRetryAfterSeconds(Guid userId, string endpoint)
    {
        var config = GetRateLimitConfig(endpoint);
        if (config == null)
            return 0;

        var resetTimeKey = GetResetTimeKey(userId, endpoint);
        if (_bucketResetTimes.TryGetValue(resetTimeKey, out var lastReset))
        {
            var elapsed = (int)(DateTime.UtcNow - lastReset).TotalSeconds;
            var remaining = config.WindowSeconds - elapsed;
            return Math.Max(0, remaining);
        }

        return config.WindowSeconds;
    }

    private RateLimitConfig? GetRateLimitConfig(string endpoint)
    {
        try
        {
            var section = _configuration.GetSection($"RateLimiting:Endpoints:{endpoint}");
            if (!section.Exists())
                return null;

            var maxRequestsStr = section["MaxRequests"];
            var windowSecondsStr = section["WindowSeconds"];

            if (!int.TryParse(maxRequestsStr, out int maxRequests) || maxRequests <= 0)
                return null;

            if (!int.TryParse(windowSecondsStr, out int windowSeconds) || windowSeconds <= 0)
                return null;

            return new RateLimitConfig { MaxRequests = maxRequests, WindowSeconds = windowSeconds };
        }
        catch
        {
            return null;
        }
    }

    private IEnumerable<string> GetConfiguredEndpoints()
    {
        var section = _configuration.GetSection("RateLimiting:Endpoints");
        if (!section.Exists())
            return Enumerable.Empty<string>();

        return section.GetChildren().Select(c => c.Key);
    }

    private bool IsRateLimitingEnabled()
    {
        var enabledStr = _configuration["RateLimiting:Enabled"];
        return bool.TryParse(enabledStr, out var enabled) && enabled;
    }

    private string GetBucketKey(Guid userId, string endpoint) => $"ratelimit:bucket:{userId}:{endpoint}";

    private string GetResetTimeKey(Guid userId, string endpoint) => $"ratelimit:reset:{userId}:{endpoint}";
}
