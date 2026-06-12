using Application.DTOs.Responses.Reports;
using MediatR;

namespace Application.Features.Reports.GetAdminReport;

public sealed record GetAdminReportQuery : IRequest<AdminReportResponse>;