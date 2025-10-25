using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace api;

public class MyGlobalExceptionHandler(ILogger<MyGlobalExceptionHandler> logger, IHostEnvironment env) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        // Log the full exception with stack trace and context
        logger.LogError(exception,
            "Unhandled exception occurred. Path: {Path}, Method: {Method}, TraceId: {TraceId}",
            httpContext.Request.Path,
            httpContext.Request.Method,
            httpContext.TraceIdentifier);

        // Determine status code based on exception type
        var statusCode = exception switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            InvalidOperationException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = GetTitle(exception),
            Detail = env.IsDevelopment() ? exception.Message : "An error occurred processing your request.",
            Instance = httpContext.Request.Path,
            Type = $"https://httpstatuses.com/{statusCode}"
        };

        // In development, include stack trace for debugging
        if (env.IsDevelopment())
        {
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            problemDetails.Extensions["innerException"] = exception.InnerException?.Message;
            problemDetails.Extensions["exceptionType"] = exception.GetType().Name;
        }

        // Add trace ID for correlation
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }

    private static string GetTitle(Exception exception) => exception switch
    {
        ArgumentException => "Bad Request",
        UnauthorizedAccessException => "Unauthorized",
        KeyNotFoundException => "Not Found",
        InvalidOperationException => "Conflict",
        _ => "Internal Server Error"
    };
}