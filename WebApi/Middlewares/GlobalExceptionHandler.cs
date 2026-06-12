using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Middlewares;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = CreateProblemDetails(httpContext, exception);

        if (problemDetails.Status == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occurred");
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception occurred");
        }

        httpContext.Response.StatusCode = problemDetails.Status!.Value;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private ProblemDetails CreateProblemDetails(HttpContext httpContext, Exception exception)
    {
        var statusCode = exception switch
        {
            KeyNotFoundException => StatusCodes.Status404NotFound,

            ArgumentException => StatusCodes.Status400BadRequest,

            ValidationException => StatusCodes.Status400BadRequest,

            InvalidOperationException => StatusCodes.Status400BadRequest,

            UnauthorizedAccessException => StatusCodes.Status403Forbidden,

            _ => StatusCodes.Status500InternalServerError
        };

        var title = statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad request",
            StatusCodes.Status403Forbidden => "Forbidden",
            StatusCodes.Status404NotFound => "Not found",
            StatusCodes.Status500InternalServerError => "Server error",
            _ => "Error"
        };

        var detail = statusCode == StatusCodes.Status500InternalServerError && !_environment.IsDevelopment()
            ? "An unexpected error occurred."
            : exception.Message;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        return problemDetails;
    }
}