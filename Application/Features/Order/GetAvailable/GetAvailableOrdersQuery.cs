using Application.DTOs.Order;
using MediatR;

namespace Application.Features.Order.GetAvailable;

public sealed class GetAvailableOrdersQuery : IRequest<IEnumerable<OrderResponse>> { }
