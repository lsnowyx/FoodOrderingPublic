using Application.DTOs.Common;
using MediatR;

namespace Application.Features.Category.Delete;

public sealed class DeleteCategoryCommand : IRequest<OperationResponse>
{
    public Guid Id { get; set; }
}
