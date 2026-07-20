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
        services.AddInfrastructureServices(configuration);
        services.AddJwtBearerInfrastructure(configuration);
        return services;
    }

    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddTransient<IAccountService, AccountService>();
        services.AddScoped<IOrderCompletionService, OrderCompletionService>();
        services.AddTransient<IJwtBearerService, JwtBearerService>();
        services.AddSingleton<CacheCircuitState>();
        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddTransient<IOrderTrackingTokenService, OrderTrackingTokenService>();
        services.AddSingleton<IDeliveryLocationCalculator, DeliveryLocationCalculator>();
        services.AddScoped<IDeliveryTrackingAccessService, DeliveryTrackingAccessService>();
        services.AddScoped<IDeliveryTrackingMapService, DeliveryTrackingMapService>();
        services.AddScoped<IStartDeliveryService, StartDeliveryService>();
        services.AddScoped<IMenuItemCostService, MenuItemCostService>();
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddHttpClient<INutritionLookupService, FoodDataCentralNutritionLookupService>();
        services.AddHttpClient<IGeocodingService, NominatimGeocodingService>();
        services.Configure<JwtBearerOptions>(configuration.GetSection("JwtBearer"));
        services.Configure<DeliverySimulationSettings>(
            configuration.GetSection(nameof(DeliverySimulationSettings)));
        services.Configure<OrderTrackingSettings>(
            configuration.GetSection(nameof(OrderTrackingSettings)));
        services.Configure<NutritionLookupSettings>(
            configuration.GetSection(nameof(NutritionLookupSettings)));
        services.Configure<EmailSettings>(
            configuration.GetSection(nameof(EmailSettings)));
        services.Configure<NominatimSettings>(
            configuration.GetSection(nameof(NominatimSettings)));
        services.Configure<RedisSettings>(
            configuration.GetSection(nameof(RedisSettings)));
        services.AddRedisInfrastructure(configuration);
        services.AddCloudinaryInfrastructure(configuration);
        services.AddIdentityInfrastructure();
        services.AddStripeInfrastructure(configuration);
        return services;
    }

    public static IServiceCollection AddRedisInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisSettings = configuration
            .GetSection(nameof(RedisSettings))
            .Get<RedisSettings>() ?? new RedisSettings();

        if (redisSettings.Enabled
            && !string.IsNullOrWhiteSpace(redisSettings.ConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisSettings.ConnectionString;
                options.InstanceName = redisSettings.InstanceName;
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        return services;
    }

    public static IServiceCollection AddStripeInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IStripePaymentService, StripePaymentService>();
        services.Configure<StripeSettings>(configuration.GetSection(nameof(StripeSettings)));
        return services;
    }

    public static IServiceCollection AddCloudinaryInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<CloudinarySettings>(configuration.GetSection(nameof(CloudinarySettings)));
        services.AddScoped<ICloudinaryService, CloudinaryService>();
        return services;
    }

    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services)
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

    public static IServiceCollection AddJwtBearerInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
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
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"].ToString();
                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrWhiteSpace(accessToken)
                            && path.StartsWithSegments(
                                global::Common.Constants.DeliveryTrackingHubConstants.Path))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });
        return services;
    }

    public static IServiceCollection AddDeliverySimulationScheduler(this IServiceCollection services)
    {
        services.AddScoped<IDeliverySimulationScheduler, DeliverySimulationScheduler>();
        return services;
    }

    public static IServiceCollection AddDeliverySimulationJobWorker(this IServiceCollection services)
    {
        services.AddScoped<IDeliverySimulationJob, DeliverySimulationJob>();
        return services;
    }
}
