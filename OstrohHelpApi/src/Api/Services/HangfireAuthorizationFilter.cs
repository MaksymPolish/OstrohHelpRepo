using Hangfire.Dashboard;

namespace Api.Services;

/// <summary>
/// Authorization filter for Hangfire dashboard access.
/// Restricts dashboard access to localhost only in development environment.
/// </summary>
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly IWebHostEnvironment _environment;

    public HangfireAuthorizationFilter(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public bool Authorize(DashboardContext context)
    {
        // Allow access in development environment from localhost only
        if (_environment.IsDevelopment())
        {
            var remoteAddress = context.Request.RemoteIpAddress;
            return remoteAddress == "127.0.0.1" || remoteAddress == "::1" || 
                   remoteAddress?.StartsWith("192.168.") == true ||
                   remoteAddress?.StartsWith("localhost") == true;
        }

        // In production, disallow dashboard access
        return false;
    }
}
