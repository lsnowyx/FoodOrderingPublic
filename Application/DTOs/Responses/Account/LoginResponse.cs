namespace Application.DTOs.Responses.Account;

public sealed record LoginResponse(string JWT, string Role, Guid UserId);
