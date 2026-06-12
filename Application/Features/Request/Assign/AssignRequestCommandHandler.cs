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
        var temp = await requestsRepository.GetByIdAsync(request.requestId);

        if (temp == null) throw new Exception("request doesn't exist");

        if (temp.WorkerId != null) throw new Exception("Already occupied");

        if (!await accountService.IsWorker(request.workerId)) throw new Exception("user is not worker");

        temp.WorkerId = request.workerId;

        await requestsRepository.SaveChangesAsync();
        return request.Adapt<AssignRequestResponse>();
    }
}