using AdminPanel.Models.Auth;
using Application.DTOs.Request;
using Mapster;

namespace AdminPanel.Mapper;

public sealed class AccountMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateAccountViewModel, CreateAdminAccountRequest>();
        config.NewConfig<LoginViewModel, LoginRequest>();
    }
}
