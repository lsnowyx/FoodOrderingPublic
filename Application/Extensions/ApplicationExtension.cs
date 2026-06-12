using Mapster;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application.Extensions;

public static class ApplicationExtension
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());

        return services;
    }
}