using Application.Extensions;
using Common.Constants;
using Domain.ConfigModels;
using Domain.Entities;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Persistence.Extensions;
using QuestPDF.Infrastructure;
using Serilog;
using WebApi.Extensions;
using WebApi.Middlewares;
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddPresentation(builder.Configuration);
builder.Services.AddApplication();

builder.Services.Configure<List<SeedUser>>
    (builder.Configuration.GetSection("SeedUsers"));
Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Services.AddLogging(logging => logging.AddSerilog(dispose: true));
builder.Host.UseSerilog();
var app = builder.Build();

#region SeedData
static async Task SeedData(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var seedUsers = scope.ServiceProvider.GetRequiredService<IOptions<List<SeedUser>>>().Value;

    foreach (var roleName in UserRoleConstants.ROLES)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
        }
    }

    foreach (var seedUser in seedUsers)
    {
        var user = await userManager.FindByNameAsync(seedUser.UserName!);
        if (user != null)
        {
            continue;
        }
        await userManager.CreateAsync(seedUser, seedUser.Password);
        await userManager.AddToRolesAsync(seedUser, seedUser.Roles);
    }
}

await SeedData(app.Services);
#endregion

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors(ConfigConstants.CorsPolicyName);

app.UseAuthentication();

app.UseMiddleware<AuditMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.UseExceptionHandler();

app.Run();