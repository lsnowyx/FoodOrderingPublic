namespace Application.DTOs.Request;

public sealed record AccessTokenRequest(string Role, Guid UserId);