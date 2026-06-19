using Application.DTOs.AuditLog;
using Domain.Entities;
using Mapster;

namespace Application.Mapper;

public class AuditLogMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateAuditLogRequest, AuditLog>()
            // Id/Timestamp are generated server-side; User navigation is not populated from audit DTOs.
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.User!)
            .Ignore(dest => dest.Timestamp);
    }
}
