using Application.DTOs.Request;
using Application.DTOs.Responses.Request;
using Application.Features.Request.Assign;
using Application.Features.Request.Create;
using Domain.Entities;
using Mapster;

namespace Application.Mapper;

public class RequestMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateRequestRequest, CreateRequestCommand>()
            // Set from the authenticated user claim by RequestsController.
            .Ignore(dest => dest.ClientId);

        config.NewConfig<CreateRequestCommand, Request>()
            // Domain defaults and EF navigation properties are not provided by the create command.
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Completed)
            .Ignore(dest => dest.Client)
            .Map(dest => dest.WorkerId, _ => (Guid?)null)
            .Ignore(dest => dest.Worker!);

        config.NewConfig<Request, CreateRequestResponse>();
        config.NewConfig<Request, GetRequestReponse>();
        config.NewConfig<AssignRequestCommand, AssignRequestResponse>();

        config.NewConfig<Request, GetRequest3>()
            .Map(dest => dest.ClientName, src => src.Client.UserName)
            .Map(dest => dest.WorkerName, src => src.Worker != null ? src.Worker.UserName : null);
    }
}
