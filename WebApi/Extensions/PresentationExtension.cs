using Common.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using WebApi.Middlewares;

namespace WebApi.Extensions;

public static class PresentationExtension
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwagger();
        services.AddCors();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        return services;
    }

    private static IServiceCollection AddCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(ConfigConstants.CorsPolicyName, policy =>
            {
                policy
                    .WithOrigins(
                        "https://localhost:7187",
                        "http://localhost:5030"
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
        return services;
    }

    private static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                BearerFormat = "JWT"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = JwtBearerDefaults.AuthenticationScheme
                    }
                },
                Array.Empty<string>()
            }
            });
        });

        services.AddAuthorization(options =>
        {
            //Admin
            options.AddPolicy(AuthorizationPolicyConstants.ADMIN_POLICY,
                policy => policy.RequireAuthenticatedUser()
                .RequireClaim(JWTClaimsConstants.ROLE, UserRoleConstants.ADMIN_ROLE));
            //User
            options.AddPolicy(AuthorizationPolicyConstants.USER_POLICY,
                policy => policy.RequireAuthenticatedUser()
                .RequireClaim(JWTClaimsConstants.ROLE, UserRoleConstants.USER_ROLE));
            //Worker
            options.AddPolicy(AuthorizationPolicyConstants.WORKER_POLICY,
                policy => policy.RequireAuthenticatedUser()
                .RequireClaim(JWTClaimsConstants.ROLE, UserRoleConstants.WORKER_ROLE));

            //Order Manager
            options.AddPolicy(AuthorizationPolicyConstants.ORDER_MANAGER_POLICY,
                policy => policy.RequireAuthenticatedUser()
                .RequireClaim(JWTClaimsConstants.ROLE, UserRoleConstants.ORDER_MANAGER_ROLE));
            //Menu Manager
            options.AddPolicy(AuthorizationPolicyConstants.MENU_MANAGER_POLICY,
                policy => policy.RequireAuthenticatedUser()
                .RequireClaim(JWTClaimsConstants.ROLE, UserRoleConstants.MENU_MANAGER_ROLE));
        });



        return services;
    }
}