using Common.Constants;
using System.Security.Claims;

namespace WebApi.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetRole(this ClaimsPrincipal claims)
    {
        return claims.FindFirstValue(JWTClaimsConstants.ROLE) ?? throw new UnauthorizedAccessException();
    }
    public static string GetUserId(this ClaimsPrincipal claims)
    {
        return claims.FindFirstValue(JWTClaimsConstants.UID) ?? throw new UnauthorizedAccessException();
    }
}
