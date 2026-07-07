using Application.Caching;
using Mapster;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application.Extensions;

public static class ApplicationExtension
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        params Assembly[] additionalMappingAssemblies)
    {
        var applicationAssembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(applicationAssembly);
        });
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachedQueryBehavior<,>));

        var mappingConfig = TypeAdapterConfig.GlobalSettings;
        mappingConfig.Scan(applicationAssembly);

        foreach (var assembly in additionalMappingAssemblies.Distinct())
        {
            if (assembly != applicationAssembly)
            {
                mappingConfig.Scan(assembly);
            }
        }

        mappingConfig.RequireDestinationMemberSource = true;
        mappingConfig.RequireExplicitMapping = true;
        mappingConfig.RequireExplicitMappingPrimitive = true;
        mappingConfig.Compile();

        return services;
    }
}
