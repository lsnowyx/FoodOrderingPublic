using Application.Abstractions.Services;
using Application.DTOs.Request;
using Common.ConfigModels;
using Common.Constants;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Services;

public class JwtBearerService : IJwtBearerService
{
    private readonly JwtBearerOptions options;

    public JwtBearerService(IOptions<JwtBearerOptions> config) => options = config.Value;

    public string GenerateAccessToken(AccessTokenRequest args)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Secret));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JWTClaimsConstants.UID, args.UserId.ToString()),
            new(JWTClaimsConstants.ROLE, args.Role)
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = options.Issuer,
            Audience = options.Audience,
            Expires = DateTime.UtcNow.AddMinutes(options.AccessExpirationInMinutes),
            SigningCredentials = credentials
        };
        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}