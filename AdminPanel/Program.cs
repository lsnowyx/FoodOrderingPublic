using AdminPanel.Constants;
using AdminPanel.Filters;
using Application.Extensions;
using Common.Constants;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Persistence.Extensions;
using System.Globalization;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<AdminPanelExceptionFilter>();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.AddService<AdminPanelExceptionFilter>();
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});
builder.Services.AddApplication(typeof(Program).Assembly);
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddHangfireBackgroundJobClient(builder.Configuration);
builder.Services.AddDeliverySimulationScheduler();

// Cookie authentication remains the default authentication mechanism for MVC.
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = AdminAuthenticationConstants.CookieScheme;
        options.DefaultChallengeScheme = AdminAuthenticationConstants.CookieScheme;
        options.DefaultSignInScheme = AdminAuthenticationConstants.CookieScheme;
    })
    .AddCookie(AdminAuthenticationConstants.CookieScheme, options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.Cookie.Name = AdminAuthenticationConstants.CookieName;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
        options.SlidingExpiration = false;
        options.Events.OnValidatePrincipal = async context =>
        {
            var role = context.Principal?.FindFirst(ClaimTypes.Role)?.Value;
            var isAllowedStaffRole = role is not null
                && UserRoleConstants.ADMIN_PANEL_ROLES.Contains(role, StringComparer.Ordinal);

            if (!isAllowedStaffRole)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(AdminAuthenticationConstants.CookieScheme);
            }
        };
    });

builder.Services.AddFoodOrderingAuthorization();

var app = builder.Build();

var adminPanelCulture = CultureInfo.GetCultureInfo("en-US");
var supportedCultures = new[] { adminPanelCulture };

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(adminPanelCulture),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
