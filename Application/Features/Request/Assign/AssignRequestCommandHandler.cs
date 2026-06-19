using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.DTOs.Responses.Request;
using Mapster;
using MediatR;

namespace Application.Features.Request.Assign;

public class AssignRequestCommandHandler : IRequestHandler<AssignRequestCommand, AssignRequestResponse>
{
    private readonly IRequestsRepository requestsRepository;
    private readonly IAccountService accountService;

    public AssignRequestCommandHandler(IRequestsRepository requestsRepository, IAccountService accountService)
    {
        this.requestsRepository = requestsRepository;
        this.accountService = accountService;
    }
    public async Task<AssignRequestResponse> Handle(AssignRequestCommand request, CancellationToken cancellationToken)
    {
        var temp = await requestsRepository.GetByIdAsync(request.requestId, cancellationToken);

        if (temp == null) throw new KeyNotFoundException("Request does not exist.");

        if (temp.WorkerId != null) throw new InvalidOperationException("Request is already assigned.");

        if (!await accountService.IsWorker(request.workerId))
            throw new ArgumentException("Selected user is not a worker.");

        temp.WorkerId = request.workerId;

        await requestsRepository.SaveChangesAsync(cancellationToken);
        return request.Adapt<AssignRequestResponse>();
    }
}
