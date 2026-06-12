using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WebClient;
using WebClient.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp =>
{
    return new HttpClient
    {
        BaseAddress = new Uri("https://localhost:7057/")
    };
});

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ApiService>();

await builder.Build().RunAsync();