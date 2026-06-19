using Application.Abstractions.Repositories;
using Application.DTOs.AuditLog;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Common.Extensions;

namespace WebApi.Middlewares;

public sealed class AuditMiddleware
{
    private const int MaxBodyLength = 10_000;

    private static readonly string[] SensitiveFields =
    [
        "password",
        "oldPassword",
        "newPassword",
        "confirmPassword",
        "token",
        "trackingToken",
        "accessToken",
        "refreshToken",
        "authorization",
        "jwt"
    ];

    private static readonly string[] AuditedMethods =
    [
        HttpMethods.Post,
        HttpMethods.Put,
        HttpMethods.Patch,
        HttpMethods.Delete
    ];

    private readonly RequestDelegate _next;
    private readonly ILogger<AuditMiddleware> _logger;

    public AuditMiddleware(
        RequestDelegate next,
        ILogger<AuditMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IAuditLogRepository auditLogService)
    {
        if (ShouldSkipAudit(context))
        {
            await _next(context);
            return;
        }

        var requestBody = await ReadRequestBodyAsync(context);
        var originalResponseBody = context.Response.Body;

        await using var responseBody = new MemoryStream();

        try
        {
            context.Response.Body = responseBody;

            await _next(context);

            var responseBodyContent = await ReadResponseBodyAsync(responseBody);

            await SaveAuditLogAsync(
                context,
                auditLogService,
                requestBody,
                responseBodyContent,
                context.Response.StatusCode,
                context.RequestAborted);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Exception occurred during audited request.");

            await SaveAuditLogAsync(
                context,
                auditLogService,
                requestBody,
                null,
                StatusCodes.Status500InternalServerError,
                CancellationToken.None);

            throw;
        }
        finally
        {
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalResponseBody);
            context.Response.Body = originalResponseBody;
        }
    }

    private static bool ShouldSkipAudit(HttpContext context)
    {
        var path = context.Request.Path;

        if (!AuditedMethods.Contains(context.Request.Method))
        {
            return true;
        }

        if (context.Request.Method == HttpMethods.Options)
        {
            return true;
        }

        if (path.StartsWithSegments("/swagger"))
        {
            return true;
        }

        if (path.StartsWithSegments("/health"))
        {
            return true;
        }

        return false;
    }

    private static async Task<string?> ReadRequestBodyAsync(HttpContext context)
    {
        if (!IsJsonContent(context.Request.ContentType))
        {
            return null;
        }

        context.Request.EnableBuffering();

        using var reader = new StreamReader(
            context.Request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync();

        context.Request.Body.Position = 0;

        return RemoveSensitiveFieldsAndLimit(body);
    }

    private static async Task<string?> ReadResponseBodyAsync(MemoryStream responseBody)
    {
        responseBody.Seek(0, SeekOrigin.Begin);

        using var reader = new StreamReader(
            responseBody,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync();

        responseBody.Seek(0, SeekOrigin.Begin);

        return RemoveSensitiveFieldsAndLimit(body);
    }

    private async Task SaveAuditLogAsync(
        HttpContext context,
        IAuditLogRepository auditLogService,
        string? requestBody,
        string? responseBody,
        int statusCode,
        CancellationToken cancellationToken)
    {
        try
        {
            var createAuditLogRequest = new CreateAuditLogRequest(
                TryGetUserId(context),
                context.Request.Method,
                context.Request.Path,
                statusCode,
                requestBody,
                responseBody);
            await auditLogService.LogAsync(createAuditLogRequest, cancellationToken);
        }
        catch (Exception auditException)
        {
            _logger.LogError(auditException, "Failed to save audit log.");
        }
    }

    private static Guid? TryGetUserId(HttpContext context)
    {
        try
        {
            return context.User.GetUserId();
        }
        catch
        {
            return null;
        }
    }

    private static bool IsJsonContent(string? contentType)
    {
        return !string.IsNullOrWhiteSpace(contentType)
            && contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase);
    }

    private static string? RemoveSensitiveFieldsAndLimit(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        var cleaned = RemoveSensitiveFields(body);

        return cleaned.Length > MaxBodyLength
            ? cleaned[..MaxBodyLength] + "...[truncated]"
            : cleaned;
    }

    private static string RemoveSensitiveFields(string body)
    {
        try
        {
            var node = JsonNode.Parse(body);

            if (node is null)
            {
                return body;
            }

            RemoveSensitiveFields(node);

            return node.ToJsonString(new JsonSerializerOptions
            {
                WriteIndented = false
            });
        }
        catch
        {
            return body;
        }
    }

    private static void RemoveSensitiveFields(JsonNode node)
    {
        if (node is JsonObject jsonObject)
        {
            foreach (var property in jsonObject.ToList())
            {
                if (SensitiveFields.Contains(property.Key, StringComparer.OrdinalIgnoreCase))
                {
                    jsonObject.Remove(property.Key);
                }
                else if (property.Value is not null)
                {
                    RemoveSensitiveFields(property.Value);
                }
            }
        }

        if (node is JsonArray jsonArray)
        {
            foreach (var item in jsonArray)
            {
                if (item is not null)
                {
                    RemoveSensitiveFields(item);
                }
            }
        }
    }
}
