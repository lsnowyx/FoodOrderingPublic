using Application.DTOs.Category;
using MediatR;

namespace Application.Features.Category.GetById;

public sealed class GetCategoryByIdQuery : IRequest<CategoryResponse?>
{
    public Guid Id { get; set; }
}
