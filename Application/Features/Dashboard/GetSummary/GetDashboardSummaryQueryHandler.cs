using Application.Abstractions.Repositories;
using Application.DTOs.Dashboard;
using MediatR;

namespace Application.Features.Dashboard.GetSummary;

public sealed class GetDashboardSummaryQueryHandler
    : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryResponse>
{
    private const int RecentOrderCount = 10;
    private readonly IOrdersRepository _ordersRepository;

    public GetDashboardSummaryQueryHandler(IOrdersRepository ordersRepository)
    {
        _ordersRepository = ordersRepository;
    }

    public Task<DashboardSummaryResponse> Handle(
        GetDashboardSummaryQuery request,
        CancellationToken cancellationToken)
    {
        return _ordersRepository.GetDashboardSummaryAsync(
            RecentOrderCount,
            cancellationToken);
    }
}
