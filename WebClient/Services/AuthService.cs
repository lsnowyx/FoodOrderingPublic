using Microsoft.JSInterop;
using System.Net.Http.Json;
using WebClient.Models;

namespace WebClient.Services;

public sealed class AuthService
{
    private readonly HttpClient httpClient;
    private readonly IJSRuntime jsRuntime;

    public event Action? AuthChanged;

    public AuthService(HttpClient httpClient, IJSRuntime jsRuntime)
    {
        this.httpClient = httpClient;
        this.jsRuntime = jsRuntime;
    }

    public async Task<bool> LoginAsync(LoginRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("api/Accounts/Login", request);

        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

        if (loginResponse is null || string.IsNullOrWhiteSpace(loginResponse.JWT))
        {
            return false;
        }

        await jsRuntime.InvokeVoidAsync("localStorage.setItem", "jwt", loginResponse.JWT);
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", "role", loginResponse.Role);

        AuthChanged?.Invoke();

        return true;
    }

    public async Task LogoutAsync()
    {
        await jsRuntime.InvokeVoidAsync("localStorage.removeItem", "jwt");
        await jsRuntime.InvokeVoidAsync("localStorage.removeItem", "role");

        AuthChanged?.Invoke();
    }

    public async Task<string?> GetJwtAsync()
    {
        return await jsRuntime.InvokeAsync<string?>("localStorage.getItem", "jwt");
    }

    public async Task<string?> GetRoleAsync()
    {
        return await jsRuntime.InvokeAsync<string?>("localStorage.getItem", "role");
    }

    public async Task<bool> IsLoggedInAsync()
    {
        var jwt = await GetJwtAsync();

        return !string.IsNullOrWhiteSpace(jwt);
    }
}