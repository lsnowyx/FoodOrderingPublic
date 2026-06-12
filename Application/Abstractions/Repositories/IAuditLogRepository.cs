using Application.DTOs.AuditLog;

namespace Application.Abstractions.Repositories;

public interface IAuditLogRepository
{
    Task LogAsync(CreateAuditLogRequest request, CancellationToken cancellationToken = default);
}
