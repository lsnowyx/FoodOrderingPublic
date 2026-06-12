using Application.DTOs.Responses.Request;
using MediatR;

namespace Application.Features.Request.Complete;

public record CompleteRequestCommand(Guid workerId, Guid requestId) : IRequest<GetRequestReponse>;