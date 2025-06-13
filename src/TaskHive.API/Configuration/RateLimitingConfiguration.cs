using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Threading.RateLimiting;

namespace TaskHive.API.Configuration;

public static class RateLimitingConfiguration
{
    public static IServiceCollection AddRateLimitingServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RateLimitingSettings>(
            configuration.GetSection(RateLimitingSettings.SectionName));

        services.AddRateLimiter(options =>
        {
            // Global rate limiter
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var settings = context.RequestServices.GetRequiredService<IOptions<RateLimitingSettings>>().Value;
                var clientIp = context.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: clientIp,
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = settings.Global.PermitLimit,
                        Window = TimeSpan.FromMinutes(settings.Global.WindowMinutes)
                    });
            });

            // SignIn specific rate limiter
            options.AddPolicy("SignInPolicy", httpContext =>
            {
                var settings = httpContext.RequestServices.GetRequiredService<IOptions<RateLimitingSettings>>().Value;
                var clientIp = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: clientIp,
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = settings.SignInPolicy.PermitLimit,
                        Window = TimeSpan.FromMinutes(settings.SignInPolicy.WindowMinutes)
                    });
            });

            // Configure rate limit exceeded response
            options.OnRejected = async (context, token) =>
            {
                var settings = context.HttpContext.RequestServices.GetRequiredService<IOptions<RateLimitingSettings>>().Value;
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";

                // Calculate retry after time
                var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterTime)
                    ? retryAfterTime.TotalSeconds
                    : settings.DefaultRetryAfterSeconds;

                // Set Retry-After header
                context.HttpContext.Response.Headers.RetryAfter = retryAfter.ToString();

                var response = new ProblemDetails
                {
                    Status = (int)HttpStatusCode.InternalServerError,
                    Title = "Too many requests.",
                    Detail = $"Please try after {retryAfter}s"
                };

                await context.HttpContext.Response.WriteAsJsonAsync(response, token);
            };
        });

        return services;
    }
} 