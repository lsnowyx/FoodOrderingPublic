using Application.Abstractions.Services;
using Application.DTOs.Responses.Account;
using Application.Features.Account.Create;
using Common.Constants;
using MediatR;

namespace Application.Features.Account.CreateStaff;

public sealed class CreateStaffAccountCommandHandler
    : IRequestHandler<CreateStaffAccountCommand, CreateAccountResponse>
{
    private readonly IAccountService _accountService;

    public CreateStaffAccountCommandHandler(IAccountService accountService)
    {
        _accountService = accountService;
    }

    public Task<CreateAccountResponse> Handle(
        CreateStaffAccountCommand request,
        CancellationToken cancellationToken)
    {
        var role = UserRoleConstants.ADMIN_CREATABLE_ROLES.FirstOrDefault(
            allowedRole => string.Equals(
                allowedRole,
                request.Role,
                StringComparison.OrdinalIgnoreCase));

        if (role is null)
        {
            throw new InvalidOperationException(
                "Only menu manager and order manager accounts can be created here.");
        }

        return _accountService.CreateAccount(new CreateAccountCommand
        {
            UserName = request.UserName,
            Password = request.Password,
            Role = role
        });
    }
}
