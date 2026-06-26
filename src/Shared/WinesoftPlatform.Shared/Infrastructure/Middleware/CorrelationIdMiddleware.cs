using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace WinesoftPlatform.API.Shared.Infrastructure.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeaderKey = "X-Correlation-Id";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(CorrelationIdHeaderKey, out var correlationId) || string.IsNullOrEmpty(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
            // Append it to request headers so subsequent code and reverse proxy forwards it
            context.Request.Headers[CorrelationIdHeaderKey] = correlationId;
        }

        // Add to response header
        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(CorrelationIdHeaderKey))
            {
                context.Response.Headers[CorrelationIdHeaderKey] = correlationId;
            }
            return Task.CompletedTask;
        });

        // Enrich the log context
        using (LogContext.PushProperty("CorrelationId", correlationId.ToString()))
        {
            await _next(context);
        }
    }
}
