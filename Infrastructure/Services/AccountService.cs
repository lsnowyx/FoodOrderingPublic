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
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        var roles = await userManager.GetRolesAsync(user);
        if (roles.Count == 0)
        {
            throw new InvalidOperationException("The account has no assigned role.");
        }

        var args = new AccessTokenRequest(roles.First(), user.Id);

        string jwt = jwtBearerService.GenerateAccessToken(args);
        var result = new LoginResponse(jwt, roles.First(), user.Id);

        return result;
    }

    public async Task<CreateAccountResponse> CreateAccount(CreateAccountCommand createAccountCommand)
    {
        var role = UserRoleConstants.ROLES.FirstOrDefault(
            allowedRole => string.Equals(
                allowedRole,
                createAccountCommand.Role,
                StringComparison.OrdinalIgnoreCase));

        if (role is null || role == UserRoleConstants.ADMIN_ROLE)
        {
            throw new InvalidOperationException("The requested account role cannot be created.");
        }

        var existingUser = await userManager.FindByNameAsync(createAccountCommand.UserName);

        if (existingUser != null)
        {
            throw new InvalidOperationException("User with such name already exists.");
        }

        var user = createAccountCommand.Adapt<User>();

        var createResult = await userManager.CreateAsync(user, createAccountCommand.Password);

        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create user: {errors}");
        }

        var addRoleResult = await userManager.AddToRoleAsync(user, role);

        if (!addRoleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            var errors = string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to add user to role: {errors}");
        }

        return new CreateAccountResponse(user.UserName!, role);
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
