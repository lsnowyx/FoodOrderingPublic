using Application.Abstractions.Repositories;
using Application.DTOs.Responses.Request;
using Common.Constants;
using Mapster;
using MediatR;

namespace Application.Features.Request.Get;

public class GetRequestCommandHandler : IRequestHandler<GetRequestCommand, List<GetRequest3>>
{
    private readonly IRequestsRepository requestsRepository;

    public GetRequestCommandHandler(IRequestsRepository requestsRepository)
    {
        this.requestsRepository = requestsRepository;
    }
    public async Task<List<GetRequest3>> Handle(GetRequestCommand request, CancellationToken cancellationToken)
    {
        IEnumerable<Domain.Entities.Request> list;
        switch (request.Role)
        {
            case UserRoleConstants.ADMIN_ROLE:
                list = await requestsRepository.GetWithNoWorkerAsync(cancellationToken);
                break;
            case UserRoleConstants.USER_ROLE:
                list = await requestsRepository.GetByClientIdAsync(request.Id);
                break;
            case UserRoleConstants.WORKER_ROLE:
                list = await requestsRepository.GetByWorkerIdAsync(request.Id);
                break;
            default: throw new Exception("401");
        }
        return list.Adapt<List<GetRequest3>>();
    }
}
