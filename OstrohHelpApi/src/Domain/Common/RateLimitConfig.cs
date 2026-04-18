namespace Domain.Common;

/// <summary>
/// Configuration for rate limiting per endpoint
/// </summary>
public record RateLimitConfig
{
    /// <summary>
    /// Maximum number of requests allowed in the time window
    /// </summary>
    public int MaxRequests { get; init; }

    /// <summary>
    /// Time window in seconds
    /// </summary>
    public int WindowSeconds { get; init; }

    /// <summary>
    /// Display name for rate limit (e.g., "100 requests per 60 seconds")
    /// </summary>
    public string DisplayName => $"{MaxRequests} requests per {WindowSeconds} seconds";
}
