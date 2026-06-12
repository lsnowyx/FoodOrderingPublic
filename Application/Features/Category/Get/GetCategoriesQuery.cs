using Application.DTOs.Category;
using MediatR;

namespace Application.Features.Category.Get;

public sealed class GetCategoriesQuery : IRequest<IEnumerable<CategoryResponse>> { }
