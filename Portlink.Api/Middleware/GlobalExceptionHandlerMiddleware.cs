using System.Diagnostics;
using System.Net;
using PortlinkApp.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace PortlinkApp.Api.Middleware;

/// <summary>
/// Global exception handler middleware that catches all exceptions and returns RFC 7807 ProblemDetails responses
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        _logger.LogError(exception, "An error occurred processing request {TraceId}: {Message}",
            traceId, exception.Message);

        var problemDetails = exception switch
        {
            BusinessException businessEx => CreateProblemDetails(
                context,
                businessEx.StatusCode,
                businessEx.GetType().Name.Replace("Exception", ""),
                businessEx.Message,
                traceId),

            _ => CreateProblemDetails(
                context,
                StatusCodes.Status500InternalServerError,
                "InternalServerError",
                _env.IsDevelopment()
                    ? exception.Message
                    : "An unexpected error occurred. Please try again later.",
                traceId)
        };

        // Add stack trace in development mode
        if (_env.IsDevelopment() && exception is not BusinessException)
        {
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    private static ProblemDetails CreateProblemDetails(
        HttpContext context,
        int statusCode,
        string title,
        string detail,
        string traceId)
    {
        return new ProblemDetails
        {
            Type = $"https://portlink.com/errors/{ConvertToKebabCase(title)}",
            Title = SplitCamelCase(title),
            Status = statusCode,
            Detail = detail,
            Instance = context.Request.Path,
            Extensions =
            {
                ["traceId"] = traceId
            }
        };
    }

    private static string ConvertToKebabCase(string input)
    {
        return string.Concat(input.Select((c, i) =>
            i > 0 && char.IsUpper(c) ? "-" + char.ToLower(c) : char.ToLower(c).ToString()));
    }

    private static string SplitCamelCase(string input)
    {
        return string.Concat(input.Select((c, i) =>
            i > 0 && char.IsUpper(c) ? " " + c : c.ToString()));
    }
}
