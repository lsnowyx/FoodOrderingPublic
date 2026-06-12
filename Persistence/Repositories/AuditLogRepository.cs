using Application.Abstractions.Repositories;
using Application.DTOs.AuditLog;
using Domain.Entities;
using Mapster;
using Persistence.Context;

namespace Persistence.Repositories
{
    internal class AuditLogRepository : IAuditLogRepository
    {
        private readonly AppDbContext context;

        public AuditLogRepository(AppDbContext context)
        {
            this.context = context;
        }
        public async Task LogAsync(CreateAuditLogRequest request, CancellationToken cancellationToken = default)
        {
            var auditLog = request.Adapt<AuditLog>();
            auditLog.Timestamp = DateTime.UtcNow;

            context.AuditLogs.Add(auditLog);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
