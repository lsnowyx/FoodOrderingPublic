using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class AdminUser : IdentityUser<Guid>
{
    public List<AuditLog> AuditLogs { get; set; } = new();
}
