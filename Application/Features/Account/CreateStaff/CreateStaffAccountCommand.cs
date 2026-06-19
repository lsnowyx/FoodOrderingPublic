using Application.DTOs.Responses.Account;
using MediatR;

namespace Application.Features.Account.CreateStaff;

public sealed class CreateStaffAccountCommand : IRequest<CreateAccountResponse>
{
    public string Role { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
}
