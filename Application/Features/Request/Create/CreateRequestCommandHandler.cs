using Application.Abstractions.Repositories;
using Application.DTOs.Responses.Request;
using Mapster;
using MediatR;

namespace Application.Features.Request.Create;

public class CreateRequestCommandHandler : IRequestHandler<CreateRequestCommand, CreateRequestResponse>
{
    private readonly IRequestsRepository requestsRepository;

    public CreateRequestCommandHandler(IRequestsRepository requestsRepository)
    {
        this.requestsRepository = requestsRepository;
    }
    public async Task<CreateRequestResponse> Handle(CreateRequestCommand request, CancellationToken cancellationToken)
    {
        var toAdd = request.Adapt<Domain.Entities.Request>();
        await requestsRepository.AddAsync(toAdd);
        await requestsRepository.SaveChangesAsync();
        var result = toAdd.Adapt<CreateRequestResponse>();
        return result;
    }
}
