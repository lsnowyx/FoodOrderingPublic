using Application.DTOs.Responses.Account;
using MediatR;

namespace Application.Features.Account.Login;

public sealed record LoginCommand(string Username, string Password) : IRequest<LoginResponse>;