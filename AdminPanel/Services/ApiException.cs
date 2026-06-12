using System.Net;

namespace AdminPanel.Services;

public class ApiException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string? Content { get; }

    public ApiException(HttpStatusCode statusCode, string? content)
        : base($"API request failed with status {(int)statusCode}")
    {
        StatusCode = statusCode;
        Content = content;
    }
}
