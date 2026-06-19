using Application.DTOs.Dashboard;
using MediatR;

namespace Application.Features.Dashboard.GetSummary;

public sealed record GetDashboardSummaryQuery : IRequest<DashboardSummaryResponse>;
