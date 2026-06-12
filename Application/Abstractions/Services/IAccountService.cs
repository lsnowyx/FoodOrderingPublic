using Application.DTOs.Responses.Account;
using Application.Features.Account.Create;
using Application.Features.Account.Login;

namespace Application.Abstractions.Services;

public interface IAccountService
{
    Task<LoginResponse> Login(LoginCommand loginCommand);
    Task<CreateAccountResponse> CreateAccount(CreateAccountCommand createAccountCommand);
    Task<List<GetAccountReponse>> GetWorkers();
    Task<bool> IsWorker(Guid? userId);
}