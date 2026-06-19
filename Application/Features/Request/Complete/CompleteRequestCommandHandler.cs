using Application.Abstractions.Repositories;
using Application.DTOs.Responses.Request;
using Mapster;
using MediatR;

namespace Application.Features.Request.Complete;

public class CompleteRequestCommandHandler : IRequestHandler<CompleteRequestCommand, GetRequestReponse>
{
    private readonly IRequestsRepository requestsRepository;

    public CompleteRequestCommandHandler(IRequestsRepository requestsRepository)
    {
        this.requestsRepository = requestsRepository;
    }
    public async Task<GetRequestReponse> Handle(CompleteRequestCommand request, CancellationToken cancellationToken)
    {
        var temp = await requestsRepository.GetByIdAsync(request.requestId, cancellationToken);
        if (temp == null) throw new KeyNotFoundException("Request does not exist.");
        if (temp.WorkerId != request.workerId)
            throw new UnauthorizedAccessException("Request is not assigned to the current worker.");

        temp.Completed = true;
        await requestsRepository.SaveChangesAsync(cancellationToken);
        return temp.Adapt<GetRequestReponse>();
    }
}
