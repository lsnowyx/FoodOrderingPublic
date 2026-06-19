using Application.DTOs.Request;
using Application.DTOs.Responses.Account;
using Application.Features.Account.Create;
using Application.Features.Account.CreateStaff;
using Application.Features.Account.Login;
using Domain.Entities;
using Mapster;

namespace Application.Mapper;

public class AccountMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateAccountRequest, CreateAccountCommand>()
            // Role is selected by the endpoint, not accepted from the public request body.
            .Ignore(dest => dest.Role);

        config.NewConfig<CreateAdminAccountRequest, CreateStaffAccountCommand>();
        config.NewConfig<LoginRequest, LoginCommand>();
        config.NewConfig<CreateAccountCommand, User>()
            // Identity/UserManager owns generated, normalized, security, and navigation fields.
            .Ignore(dest => dest.Id)
            .Map(dest => dest.NormalizedUserName, _ => (string?)null)
            .Map(dest => dest.Email, _ => (string?)null)
            .Map(dest => dest.NormalizedEmail, _ => (string?)null)
            .Ignore(dest => dest.EmailConfirmed)
            .Map(dest => dest.PasswordHash, _ => (string?)null)
            .Map(dest => dest.SecurityStamp, _ => (string?)null)
            .Map(dest => dest.ConcurrencyStamp, _ => (string?)null)
            .Map(dest => dest.PhoneNumber, _ => (string?)null)
            .Ignore(dest => dest.PhoneNumberConfirmed)
            .Ignore(dest => dest.TwoFactorEnabled)
            .Map(dest => dest.LockoutEnd, _ => (DateTimeOffset?)null)
            .Ignore(dest => dest.LockoutEnabled)
            .Ignore(dest => dest.AccessFailedCount)
            .Map(dest => dest.Address, _ => (string?)null)
            .Ignore(dest => dest.Orders)
            .Ignore(dest => dest.Cart!)
            .Ignore(dest => dest.AuditLogs)
            .Ignore(dest => dest.ClientRequest)
            .Ignore(dest => dest.WorkerRequest);

        config.NewConfig<User, GetAccountReponse>();
    }
}
