using Application.DTOs.Common;
using MediatR;

namespace Application.Features.MenuItem.Delete;

public sealed class DeleteMenuItemCommand : IRequest<OperationResponse>
{
    public Guid Id { get; set; }
}
