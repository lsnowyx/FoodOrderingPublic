using Application.Abstractions.Services;
using Application.DTOs.Responses.Account;
using MediatR;

namespace Application.Features.Account.Create;

public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, CreateAccountResponse>
{
    private readonly IAccountService accountService;

    public CreateAccountCommandHandler(IAccountService accountService)
    {
        this.accountService = accountService;
    }
    public Task<CreateAccountResponse> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        return accountService.CreateAccount(request);
    }
}
