using Common.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class FoodOrderingAuthorizationExtensions
{
    public static IServiceCollection AddFoodOrderingAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                AuthorizationPolicyConstants.ADMIN_POLICY,
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireClaim(JWTClaimsConstants.ROLE, UserRoleConstants.ADMIN_ROLE));

            options.AddPolicy(
                AuthorizationPolicyConstants.USER_POLICY,
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireClaim(JWTClaimsConstants.ROLE, UserRoleConstants.USER_ROLE));

            options.AddPolicy(
                AuthorizationPolicyConstants.WORKER_POLICY,
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireClaim(JWTClaimsConstants.ROLE, UserRoleConstants.WORKER_ROLE));

            options.AddPolicy(
                AuthorizationPolicyConstants.ORDER_MANAGER_POLICY,
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireClaim(JWTClaimsConstants.ROLE, UserRoleConstants.ORDER_MANAGER_ROLE));

            options.AddPolicy(
                AuthorizationPolicyConstants.MENU_MANAGER_POLICY,
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireClaim(JWTClaimsConstants.ROLE, UserRoleConstants.MENU_MANAGER_ROLE));
        });

        return services;
    }
}
