using AdminPanel;
using AdminPanel.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// configuration
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));

// register HttpContextAccessor for ApiClient to access user claims
builder.Services.AddHttpContextAccessor();

// register named HttpClient for API with BaseAddress from config
var apiBase = builder.Configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "https://localhost:7057";
builder.Services.AddHttpClient("ApiClient", client => client.BaseAddress = new Uri(apiBase))
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        // In Development accept untrusted local certs to avoid SSL trust issues with localhost dev certificates.
        if (builder.Environment.IsDevelopment())
        {
            return new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
        }

        return new HttpClientHandler();
    });

// register ApiClient
builder.Services.AddScoped<IApiClient, ApiClient>();

// Cookie authentication
builder.Services.AddAuthentication("AdminCookie")
    .AddCookie("AdminCookie", options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.Cookie.Name = "AdminPanel.Auth";
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
