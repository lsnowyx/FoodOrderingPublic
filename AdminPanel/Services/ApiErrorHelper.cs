using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AdminPanel.Services;

public static class ApiErrorHelper
{
    public static void AddErrorsToModelState(ModelStateDictionary modelState, string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            modelState.AddModelError(string.Empty, "API request failed");
            return;
        }

        try
        {
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("errors", out var errors))
            {
                foreach (var prop in errors.EnumerateObject())
                {
                    var key = prop.Name;
                    if (prop.Value.ValueKind == JsonValueKind.Array)
                    {
                        var messages = prop.Value.EnumerateArray().Select(e => e.GetString() ?? string.Empty);
                        modelState.AddModelError(key, string.Join("; ", messages));
                    }
                }
                return;
            }

            // fallback: try to read title/message
            if (doc.RootElement.TryGetProperty("title", out var title))
            {
                modelState.AddModelError(string.Empty, title.GetString());
                return;
            }

            modelState.AddModelError(string.Empty, content);
        }
        catch
        {
            modelState.AddModelError(string.Empty, content);
        }
    }
}
