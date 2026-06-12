using Application.DTOs.Order;
using MediatR;

namespace Application.Features.Order.Get;

public sealed class GetOrdersQuery : IRequest<IEnumerable<OrderResponse>> { }
