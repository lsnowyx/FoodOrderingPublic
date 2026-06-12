using Application.Abstractions.Services;
using Application.DTOs.Responses.Account;
using MediatR;

namespace Application.Features.Account.Get;


public class GetAccountQueryHandler : IRequestHandler<GetAccountQuery, List<GetAccountReponse>>
{
    private readonly IAccountService accountService;

    public GetAccountQueryHandler(IAccountService accountService)
    {
        this.accountService = accountService;
    }
    public Task<List<GetAccountReponse>> Handle(GetAccountQuery request, CancellationToken cancellationToken)
    {
        return accountService.GetWorkers();
    }
}