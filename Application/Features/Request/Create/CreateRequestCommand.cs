using Application.DTOs.Responses.Request;
using MediatR;

namespace Application.Features.Request.Create;

public sealed class CreateRequestCommand : IRequest<CreateRequestResponse>
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public Guid ClientId { get; set; }
}