using Application.DTOs.Responses.Request;
using MediatR;

namespace Application.Features.Request.Assign;

public record AssignRequestCommand(Guid workerId, Guid requestId) : IRequest<AssignRequestResponse>;