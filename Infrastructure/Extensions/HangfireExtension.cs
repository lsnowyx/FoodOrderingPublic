using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class HangfireExtension
{
    public static IServiceCollection AddHangfireBackgroundProcessing(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHangfireStorage(configuration);
        services.AddHangfireServer();

        return services;
    }

    public static IServiceCollection AddHangfireBackgroundJobClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHangfireStorage(configuration);

        return services;
    }

    private static IServiceCollection AddHangfireStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var hangfireConnectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection configuration is missing.");

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(hangfireConnectionString));

        return services;
    }
}
