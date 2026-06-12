namespace Application.DTOs.AuditLog;
public record CreateAuditLogRequest(Guid? UserId, string HttpMethod, string Endpoint, int StatusCode, string? RequestBody, string? ResponseBody);