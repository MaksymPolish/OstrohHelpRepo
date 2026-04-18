using Application.Common.Services;
using System.Security.Claims;

namespace Api.Middleware;

/// <summary>
/// Middleware for enforcing rate limiting
/// Returns 429 Too Many Requests when user exceeds rate limit
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRateLimitingService _rateLimitingService;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    // Map of controller actions to rate limit endpoint names
    private static readonly Dictionary<string, string> RateLimitedEndpoints = new(StringComparer.OrdinalIgnoreCase)
    {
        { "post/api/message/send", "SendMessage" },
        { "post/api/message/batchupload", "BatchUpload" },
        { "put/api/message/mark-as-read", "MarkAsRead" },
        { "delete/api/message/delete", "DeleteMessage" },
        { "delete/api/message/attachment", "DeleteAttachment" },
    };

    public RateLimitingMiddleware(RequestDelegate next, IRateLimitingService rateLimitingService, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _rateLimitingService = rateLimitingService ?? throw new ArgumentNullException(nameof(rateLimitingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = GetEndpointName(context.Request);

        // Only check rate limit for configured endpoints
        if (endpoint != null && context.User?.FindFirst(ClaimTypes.NameIdentifier) is not null)
        {
            var userIdStr = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdStr, out var userId))
            {
                var canMakeRequest = _rateLimitingService.CanMakeRequest(userId, endpoint);

                if (!canMakeRequest)
                {
                    var retryAfter = _rateLimitingService.GetRetryAfterSeconds(userId, endpoint);
                    _logger.LogWarning(
                        "Rate limit exceeded for user {UserId} on endpoint {Endpoint}. Retry after {RetryAfter} seconds",
                        userId, endpoint, retryAfter);

                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.Response.Headers.Add("Retry-After", retryAfter.ToString());
                    context.Response.Headers.Add("X-RateLimit-Limit", GetRateLimitConfig(endpoint)?.MaxRequests.ToString() ?? "N/A");
                    context.Response.Headers.Add("X-RateLimit-Remaining", "0");
                    context.Response.Headers.Add("X-RateLimit-Reset", new DateTimeOffset(DateTime.UtcNow.AddSeconds(retryAfter)).ToUnixTimeSeconds().ToString());

                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "Rate limit exceeded",
                        message = $"Too many requests to {endpoint}. Please try again in {retryAfter} seconds.",
                        retryAfter = retryAfter
                    });

                    return;
                }

                // Add rate limit info to response headers
                var remaining = _rateLimitingService.GetRemainingRequests(userId, endpoint);
                context.Response.OnStarting(() =>
                {
                    var config = GetRateLimitConfig(endpoint);
                    if (config != null)
                    {
                        context.Response.Headers.Add("X-RateLimit-Limit", config.MaxRequests.ToString());
                        context.Response.Headers.Add("X-RateLimit-Remaining", remaining.ToString());
                        context.Response.Headers.Add("X-RateLimit-Reset", new DateTimeOffset(DateTime.UtcNow.AddSeconds(config.WindowSeconds)).ToUnixTimeSeconds().ToString());
                    }
                    return Task.CompletedTask;
                });
            }
        }

        await _next(context);
    }

    private string? GetEndpointName(HttpRequest request)
    {
        var routeKey = $"{request.Method.ToLower()}{request.Path}".ToLower();

        // Direct match
        if (RateLimitedEndpoints.TryGetValue(routeKey, out var endpoint))
            return endpoint;

        // Partial match for routes with parameters (e.g., /api/message/attachment/123)
        foreach (var (pattern, name) in RateLimitedEndpoints)
        {
            if (routeKey.StartsWith(pattern.Split('{')[0]))
                return name;
        }

        return null;
    }

    private Domain.Common.RateLimitConfig? GetRateLimitConfig(string endpoint)
    {
        // This is a simplified version - ideally we'd inject the config
        // For now, we'll return null to indicate no specific config
        return null;
    }
}

/// <summary>
/// Extension method for easy middleware registration
/// </summary>
public static class RateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RateLimitingMiddleware>();
    }
}
