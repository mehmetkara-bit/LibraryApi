using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using LibraryApi.Exceptions;

namespace LibraryApi.Middlewares;

public sealed class GlobalExceptionMiddleware : IMiddleware
{
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(ILogger<GlobalExceptionMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
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

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var statusCode = (int)HttpStatusCode.InternalServerError;
        var title = "Unexpected error";
        var detail = "An unexpected error occurred.";

        switch (ex)
        {
            case NotFoundException nf:
                statusCode = StatusCodes.Status404NotFound;
                title = "Not Found";
                detail = nf.Message;
                break;

            case ValidationException ve:
                statusCode = StatusCodes.Status400BadRequest;
                title = "Validation Error";
                detail = ve.Message;
                break;

            case ConflictException ce:
                statusCode = StatusCodes.Status409Conflict;
                title = "Conflict";
                detail = ce.Message;
                break;

            case UnauthorizedAccessException:
                statusCode = StatusCodes.Status401Unauthorized;
                title = "Unauthorized";
                detail = "Authentication is required.";
                break;

            default:
                _logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}", context.TraceIdentifier);
                break;
        }

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        problem.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}