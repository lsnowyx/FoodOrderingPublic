using Common.Constants;
using System.Security.Claims;

namespace Common.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var value = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst(JWTClaimsConstants.UID)?.Value
            ?? principal.FindFirst(ClaimConstants.UserId)?.Value;

        if (!Guid.TryParse(value, out var userId))
        {
            throw new UnauthorizedAccessException("Authenticated user id claim is missing or invalid.");
        }

        return userId;
    }

    public static string GetRole(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        return principal.FindFirst(ClaimTypes.Role)?.Value
            ?? throw new UnauthorizedAccessException("Authenticated user role claim is missing.");
    }
}
