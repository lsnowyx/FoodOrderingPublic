using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.DTOs.Request;
using Application.DTOs.Responses.Account;
using Application.Features.Account.Create;
using Application.Features.Account.Login;
using Common.Constants;
using Domain.Entities;
using Mapster;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services;

public class AccountService : IAccountService
{
    private readonly IJwtBearerService jwtBearerService;
    private readonly UserManager<User> userManager;
    private readonly IRequestsRepository requestsRepository;

    public AccountService(IJwtBearerService jwtBearerService, UserManager<User> userManager, IRequestsRepository requestsRepository)
    {
        this.jwtBearerService = jwtBearerService;
        this.userManager = userManager;
        this.requestsRepository = requestsRepository;
    }

    public async Task<LoginResponse> Login(LoginCommand loginCommand)
    {
        var user = await userManager.FindByNameAsync(loginCommand.Username);

        if (user is null || !await userManager.CheckPasswordAsync(user, loginCommand.Password))
        {
            throw new Exception("Invalid login");
        }

        var roles = await userManager.GetRolesAsync(user);

        var args = new AccessTokenRequest(roles.First(), user.Id);

        string jwt = jwtBearerService.GenerateAccessToken(args);
        var result = new LoginResponse(jwt, roles.First());

        return result;
    }

    public async Task<CreateAccountResponse> CreateAccount(CreateAccountCommand createAccountCommand)
    {
        var existingUser = await userManager.FindByNameAsync(createAccountCommand.UserName);

        if (existingUser != null)
        {
            throw new Exception("User with such name already exists");
        }

        var user = createAccountCommand.Adapt<User>();

        var createResult = await userManager.CreateAsync(user, createAccountCommand.Password);

        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            throw new Exception($"Failed to create user: {errors}");
        }

        var addRoleResult = await userManager.AddToRoleAsync(user, createAccountCommand.Role);

        if (!addRoleResult.Succeeded)
        {
            var errors = string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
            throw new Exception($"Failed to add user to role: {errors}");
        }

        return new CreateAccountResponse(user.UserName!, createAccountCommand.Role);
    }

    public async Task<List<GetAccountReponse>> GetWorkers()
    {
        var workers = await requestsRepository.GetWorkersWithoutRequestsAsync();
        var result = workers.Adapt<List<GetAccountReponse>>();
        return result;
    }

    public async Task<bool> IsWorker(Guid? userId)
    {
        if (userId == null) return false;

        var user = await userManager.FindByIdAsync(userId.ToString()!);

        if (user == null)
            return false;

        return await userManager.IsInRoleAsync(user, UserRoleConstants.WORKER_ROLE);
    }
}
