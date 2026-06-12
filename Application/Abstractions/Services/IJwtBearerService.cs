using Application.DTOs.Request;

namespace Application.Abstractions.Services;

public interface IJwtBearerService
{
    string GenerateAccessToken(AccessTokenRequest args);
}
