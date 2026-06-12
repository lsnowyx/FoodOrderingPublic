using Application.Abstractions.Services;
using Application.DTOs.Responses.Account;
using MediatR;

namespace Application.Features.Account.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IAccountService accountService;

    public LoginCommandHandler(IAccountService accountService)
    {
        this.accountService = accountService;
    }
    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return await accountService.Login(request);
    }
}
