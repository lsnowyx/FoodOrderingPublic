using Application.DTOs.Responses.Request;
using MediatR;

namespace Application.Features.Request.Get;

public sealed record GetRequestCommand(string Role, Guid Id) : IRequest<List<GetRequest3>>;