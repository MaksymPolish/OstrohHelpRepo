namespace Application.Common.Services;

/// <summary>
/// Service for rate limiting per user and endpoint
/// Implements token bucket algorithm for request throttling
/// </summary>
public interface IRateLimitingService
{
    /// <summary>
    /// Check if user can make a request to the specified endpoint
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="endpoint">API endpoint name (e.g., "SendMessage", "BatchUpload")</param>
    /// <returns>True if request is allowed, false if rate limit exceeded</returns>
    bool CanMakeRequest(Guid userId, string endpoint);

    /// <summary>
    /// Get the number of remaining requests for a user on an endpoint
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="endpoint">API endpoint name</param>
    /// <returns>Number of remaining requests (can be negative if over limit)</returns>
    int GetRemainingRequests(Guid userId, string endpoint);

    /// <summary>
    /// Reset rate limit counters for a specific user across all endpoints
    /// </summary>
    /// <param name="userId">User identifier</param>
    void ResetUserLimits(Guid userId);

    /// <summary>
    /// Get retry-after seconds for a rate-limited user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="endpoint">API endpoint name</param>
    /// <returns>Seconds to wait before retry</returns>
    int GetRetryAfterSeconds(Guid userId, string endpoint);
}
