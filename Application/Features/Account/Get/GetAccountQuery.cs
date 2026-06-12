using Application.DTOs.Responses.Account;
using MediatR;

namespace Application.Features.Account.Get;

public record GetAccountQuery : IRequest<List<GetAccountReponse>>;