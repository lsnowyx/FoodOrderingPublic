using Microsoft.AspNetCore.Http;
using AdminPanel.Models.Category;
using AdminPanel.Models.MenuItem;
using AdminPanel.Models.Order;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AdminPanel.Services;

public class ApiClient : IApiClient
{
    private readonly IHttpClientFactory _factory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiClient(IHttpClientFactory factory, IHttpContextAccessor httpContextAccessor)
    {
        _factory = factory;
        _httpContextAccessor = httpContextAccessor;
    }

    private HttpClient CreateClient()
    {
        var client = _factory.CreateClient("ApiClient");

        // attach bearer token from claims if present
        var user = _httpContextAccessor.HttpContext?.User;
        var token = user?.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;
        if (!string.IsNullOrWhiteSpace(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return client;
    }

    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<T?> GetAsync<T>(string url)
    {
        var client = CreateClient();
        var resp = await client.GetAsync(url);
        if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException();
        if (resp.StatusCode == System.Net.HttpStatusCode.Forbidden)
            throw new ForbiddenException();
        if (!resp.IsSuccessStatusCode)
        {
            var errorContent = await resp.Content.ReadAsStringAsync();
            throw new ApiException(resp.StatusCode, errorContent);
        }

        if (resp.StatusCode == System.Net.HttpStatusCode.NoContent) return default;

        var content = await resp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, _jsonOptions);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest data)
    {
        var client = CreateClient();
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        var resp = await client.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
        if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException();
        if (resp.StatusCode == System.Net.HttpStatusCode.Forbidden)
            throw new ForbiddenException();
        if (!resp.IsSuccessStatusCode)
        {
            var errorContent = await resp.Content.ReadAsStringAsync();
            throw new ApiException(resp.StatusCode, errorContent);
        }

        if (resp.StatusCode == System.Net.HttpStatusCode.NoContent) return default;

        var content = await resp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TResponse>(content, _jsonOptions);
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest data)
    {
        var client = CreateClient();
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        var resp = await client.PutAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
        if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException();
        if (resp.StatusCode == System.Net.HttpStatusCode.Forbidden)
            throw new ForbiddenException();
        if (!resp.IsSuccessStatusCode)
        {
            var errorContent = await resp.Content.ReadAsStringAsync();
            throw new ApiException(resp.StatusCode, errorContent);
        }

        if (resp.StatusCode == System.Net.HttpStatusCode.NoContent) return default;

        var content = await resp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TResponse>(content, _jsonOptions);
    }

    public async Task<TResponse?> PatchAsync<TRequest, TResponse>(string url, TRequest data)
    {
        var client = CreateClient();
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        var request = new HttpRequestMessage(new HttpMethod("PATCH"), url)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        var resp = await client.SendAsync(request);
        if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException();
        if (resp.StatusCode == System.Net.HttpStatusCode.Forbidden)
            throw new ForbiddenException();
        if (!resp.IsSuccessStatusCode)
        {
            var errorContent = await resp.Content.ReadAsStringAsync();
            throw new ApiException(resp.StatusCode, errorContent);
        }

        if (resp.StatusCode == System.Net.HttpStatusCode.NoContent) return default;

        var content = await resp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TResponse>(content, _jsonOptions);
    }

    public async Task<MenuItemPictureViewModel?> UploadMenuItemPictureAsync(Guid menuItemId, IFormFile imageFile, string? caption)
    {
        using var content = CreatePictureFormData(imageFile, caption);
        return await SendMultipartAsync<MenuItemPictureViewModel>($"api/menu-items/{menuItemId}/pictures", HttpMethod.Post, content);
    }

    public async Task<MenuItemPictureViewModel?> UpdateMenuItemPictureAsync(Guid menuItemId, Guid pictureId, IFormFile imageFile, string? caption)
    {
        using var content = CreatePictureFormData(imageFile, caption);
        return await SendMultipartAsync<MenuItemPictureViewModel>($"api/menu-items/{menuItemId}/pictures/{pictureId}", HttpMethod.Put, content);
    }

    private static MultipartFormDataContent CreatePictureFormData(IFormFile imageFile, string? caption)
    {
        var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(imageFile.OpenReadStream());
        if (!string.IsNullOrWhiteSpace(imageFile.ContentType))
        {
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(imageFile.ContentType);
        }
        content.Add(fileContent, "ImageFile", imageFile.FileName);

        if (!string.IsNullOrWhiteSpace(caption))
        {
            content.Add(new StringContent(caption), "Caption");
        }

        return content;
    }

    private async Task<TResponse?> SendMultipartAsync<TResponse>(string url, HttpMethod method, MultipartFormDataContent content)
    {
        var client = CreateClient();
        using var request = new HttpRequestMessage(method, url)
        {
            Content = content
        };

        var resp = await client.SendAsync(request);
        if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException();
        if (resp.StatusCode == System.Net.HttpStatusCode.Forbidden)
            throw new ForbiddenException();
        if (!resp.IsSuccessStatusCode)
        {
            var errorContent = await resp.Content.ReadAsStringAsync();
            throw new ApiException(resp.StatusCode, errorContent);
        }

        if (resp.StatusCode == System.Net.HttpStatusCode.NoContent) return default;

        var responseContent = await resp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TResponse>(responseContent, _jsonOptions);
    }

    public async Task<CategoryViewModel?> CreateCategoryAsync(CategoryViewModel model)
    {
        using var content = CreateCategoryFormData(model);
        return await SendMultipartAsync<CategoryViewModel>("api/categories", HttpMethod.Post, content);
    }

    public async Task<CategoryViewModel?> UpdateCategoryAsync(Guid categoryId, CategoryViewModel model)
    {
        using var content = CreateCategoryFormData(model);
        return await SendMultipartAsync<CategoryViewModel>($"api/categories/{categoryId}", HttpMethod.Put, content);
    }

    private static MultipartFormDataContent CreateCategoryFormData(CategoryViewModel model)
    {
        var content = new MultipartFormDataContent
        {
            { new StringContent(model.Name), "Name" }
        };

        if (!string.IsNullOrWhiteSpace(model.Description))
        {
            content.Add(new StringContent(model.Description), "Description");
        }

        if (model.ImageFile is not null)
        {
            var fileContent = new StreamContent(model.ImageFile.OpenReadStream());
            if (!string.IsNullOrWhiteSpace(model.ImageFile.ContentType))
            {
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(model.ImageFile.ContentType);
            }

            content.Add(fileContent, "ImageFile", model.ImageFile.FileName);
        }

        return content;
    }

    public async Task<bool> DeleteAsync(string url)
    {
        var client = CreateClient();
        var resp = await client.DeleteAsync(url);
        if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException();
        if (resp.StatusCode == System.Net.HttpStatusCode.Forbidden)
            throw new ForbiddenException();
        if (!resp.IsSuccessStatusCode)
        {
            var content = await resp.Content.ReadAsStringAsync();
            throw new ApiException(resp.StatusCode, content);
        }

        return true;
    }

    public Task<IEnumerable<OrderViewModel>?> GetAvailableOrdersAsync()
    {
        return GetAsync<IEnumerable<OrderViewModel>>("api/orders/available");
    }

    public Task<OrderViewModel?> TakeOrderAsync(Guid orderId)
    {
        return PatchAsync<object, OrderViewModel>($"api/orders/{orderId}/take", new object());
    }

    public Task<OrderViewModel?> GetMyDeliveryAsync()
    {
        return GetAsync<OrderViewModel>("api/orders/my-delivery");
    }

    public async Task StartDeliveryAsync(Guid orderId)
    {
        await PatchAsync<object, object>($"api/orders/{orderId}/start-delivery", new object());
    }

    public async Task MarkDeliveredAsync(Guid orderId)
    {
        await PatchAsync<object, object>($"api/orders/{orderId}/mark-delivered", new object());
    }
}
