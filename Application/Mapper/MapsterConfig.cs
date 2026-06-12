using Application.DTOs.Responses.Request;
using Domain.Entities;
using Mapster;

namespace Application.Mapper;


public class RequestMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Request, GetRequest3>()
            .Map(dest => dest.ClientName, src => src.Client.UserName)
            .Map(dest => dest.WorkerName, src => src.Worker != null ? src.Worker.UserName : null);
    }
}