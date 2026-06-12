using Application.Abstractions.Services;
using Domain.ConfigModels;
using Domain.Entities;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Persistence.Context;
using System.Text;
using JwtBearerOptions = Common.ConfigModels.JwtBearerOptions;


namespace Infrastructure.Extensions;

public static class InfrastructureExtension
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IAccountService, AccountService>();
        services.AddTransient<IJwtBearerService, JwtBearerService>();
        services.Configure<CloudinarySettings>(configuration.GetSection(nameof(CloudinarySettings)));

        services.AddScoped<ICloudinaryService, CloudinaryService>();

        AddIdentity(services, configuration);
        AddJwtBearer(services, configuration);
        return services;
    }
    private static IServiceCollection AddIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentityCore<User>(options =>
        {
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredUniqueChars = 0;

            options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ.'";
        })
        .AddRoles<IdentityRole<Guid>>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddSignInManager()
        .AddDefaultTokenProviders();
        return services;
    }

    private static IServiceCollection AddJwtBearer(this IServiceCollection services, IConfiguration configuration)
    {
        var jwt = configuration.GetSection("JwtBearer").Get<JwtBearerOptions>()
          ?? throw new InvalidOperationException("JwtBearer configuration is missing.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
                options.MapInboundClaims = false;
            });
        services.Configure<JwtBearerOptions>(configuration.GetSection("JwtBearer"));
        return services;
    }
}
