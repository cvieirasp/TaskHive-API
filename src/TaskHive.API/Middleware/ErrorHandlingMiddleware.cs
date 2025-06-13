using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;
using TaskHive.Domain.Exceptions;
using TaskHive.Application.Exceptions;

namespace TaskHive.API.Middleware;

public class ErrorHandlingMiddleware(
    RequestDelegate next,
    ILogger<ErrorHandlingMiddleware> logger,
    IWebHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var problemDetails = new ProblemDetails
        {
            Status = (int)HttpStatusCode.InternalServerError,
            Title = "An error occurred while processing your request.",
            Detail = environment.IsDevelopment() ? exception.Message : "An unexpected error occurred."
        };

        switch (exception)
        {
            case EmailAlreadyInUseException emailEx:
                response.StatusCode = (int)HttpStatusCode.Conflict;
                problemDetails.Status = response.StatusCode;
                problemDetails.Title = "Email already in use";
                problemDetails.Detail = emailEx.Message;
                problemDetails.Extensions["email"] = emailEx.Email;
                break;

            case InvalidCredentialsException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                problemDetails.Status = response.StatusCode;
                problemDetails.Title = "Authentication failed";
                problemDetails.Detail = exception.Message;
                break;

            case DomainException domainEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                problemDetails.Status = response.StatusCode;
                problemDetails.Title = "Domain validation error";
                problemDetails.Detail = domainEx.Message;
                break;

            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                problemDetails.Status = response.StatusCode;
                problemDetails.Title = "Unauthorized access";
                problemDetails.Detail = "You are not authorized to access this resource.";
                break;

            case JsonException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                problemDetails.Status = response.StatusCode;
                problemDetails.Title = "Invalid JSON";
                problemDetails.Detail = "The request body contains invalid JSON.";
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                logger.LogError(exception, "An unexpected error occurred");
                break;
        }

        // Add trace ID for debugging
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        // Add stack trace in development
        if (environment.IsDevelopment())
        {
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        var result = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await response.WriteAsync(result);
    }
} 