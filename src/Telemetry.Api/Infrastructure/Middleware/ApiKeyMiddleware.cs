using System.Net;

namespace Telemetry.Api.Infrastructure.Middleware;

/// <summary>
/// Validates the Authorization: Bearer header against the configured ApiKey.
/// When ApiKey is null or empty the middleware is bypassed (open access).
/// </summary>
public sealed class ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<ApiKeyMiddleware> logger)
{
    private const string Scheme = "Bearer ";

    public async Task InvokeAsync(HttpContext context)
    {
        string? requiredKey = configuration.GetValue<string>("ApiKey");

        if (!string.IsNullOrWhiteSpace(requiredKey))
        {
            string? authHeader = context.Request.Headers.Authorization.FirstOrDefault();

            if (authHeader is null || !authHeader.StartsWith(Scheme, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogWarning("Rejected request to {Path}: missing or invalid Authorization header", context.Request.Path);
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return;
            }

            string providedKey = authHeader[Scheme.Length..].Trim();

            if (!string.Equals(providedKey, requiredKey, StringComparison.Ordinal))
            {
                logger.LogWarning("Rejected request to {Path}: invalid API key", context.Request.Path);
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return;
            }
        }

        await next(context);
    }
}
