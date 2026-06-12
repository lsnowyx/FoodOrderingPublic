using System.Net.Http.Headers;
using System.Net.Http.Json;
using WebClient.Models;

namespace WebClient.Services;

public sealed class ApiService
{
    private readonly HttpClient httpClient;
    private readonly AuthService authService;

    public ApiService(HttpClient httpClient, AuthService authService)
    {
        this.httpClient = httpClient;
        this.authService = authService;
    }

    private async Task AddJwtAsync()
    {
        var jwt = await authService.GetJwtAsync();

        if (!string.IsNullOrWhiteSpace(jwt))
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwt);
        }
        else
        {
            httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    private static async Task<T> ReadResponseAsync<T>(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(error))
            {
                error = response.ReasonPhrase ?? "Request failed.";
            }

            throw new Exception(error);
        }

        var result = await response.Content.ReadFromJsonAsync<T>();

        if (result is null)
        {
            throw new Exception("Empty response from server.");
        }

        return result;
    }

    public async Task<CreateAccountResponse> CreateUserAsync(CreateAccountRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("api/Accounts/CreateUser", request);

        return await ReadResponseAsync<CreateAccountResponse>(response);
    }

    public async Task<CreateAccountResponse> CreateWorkerAsync(CreateAccountRequest request)
    {
        await AddJwtAsync();

        var response = await httpClient.PostAsJsonAsync("api/Accounts/CreateWorker", request);

        return await ReadResponseAsync<CreateAccountResponse>(response);
    }

    public async Task<List<GetWorkerResponse>> GetWorkersAsync()
    {
        await AddJwtAsync();

        var result = await httpClient.GetFromJsonAsync<List<GetWorkerResponse>>("api/Accounts/GetWorkers");

        return result ?? new List<GetWorkerResponse>();
    }

    public async Task<CreateRequestResponse> CreateRequestAsync(CreateRequestRequest request)
    {
        await AddJwtAsync();

        var response = await httpClient.PostAsJsonAsync("api/Requests/CreateRequest", request);

        return await ReadResponseAsync<CreateRequestResponse>(response);
    }

    public async Task<List<GetRequestResponse>> GetRequestsAsync()
    {
        await AddJwtAsync();

        var result = await httpClient.GetFromJsonAsync<List<GetRequestResponse>>("api/Requests/GetRequests");

        return result ?? new List<GetRequestResponse>();
    }

    public async Task<AssignRequestResponse> AssignRequestAsync(Guid requestId, Guid workerId)
    {
        await AddJwtAsync();

        var response = await httpClient.PatchAsync(
            $"api/Requests/AssignRequest/{requestId}/assign/{workerId}",
            null);

        return await ReadResponseAsync<AssignRequestResponse>(response);
    }

    public async Task<CompleteRequestResponse> CompleteRequestAsync(Guid requestId)
    {
        await AddJwtAsync();

        var response = await httpClient.PatchAsync(
            $"api/Requests/CompleteRequest/{requestId}",
            null);

        return await ReadResponseAsync<CompleteRequestResponse>(response);
    }

    public async Task<AdminReportResponse> GetAdminReportAsync()
    {
        await AddJwtAsync();

        var result = await httpClient.GetFromJsonAsync<AdminReportResponse>(
            "api/Reports/GetAdminReport");

        return result ?? new AdminReportResponse();
    }

    public async Task<byte[]> ExportAdminReportPdfAsync()
    {
        await AddJwtAsync();

        var response = await httpClient.GetAsync("api/Reports/ExportAdminReportPdf");

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(error))
            {
                error = response.ReasonPhrase ?? "Failed to export PDF.";
            }

            throw new Exception(error);
        }

        return await response.Content.ReadAsByteArrayAsync();
    }
}