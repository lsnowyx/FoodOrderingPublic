using Application.Features.Reports.GetAdminReport;
using Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using WebApi.Documents;

namespace WebApi.Controllers;

[Route("api/[controller]/[Action]")]
[ApiController]
public sealed class ReportsController : ControllerBase
{
    private readonly IMediator mediator;

    public ReportsController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    [HttpGet]
    [Authorize(AuthorizationPolicyConstants.ADMIN_POLICY)]
    public async Task<IActionResult> GetAdminReport()
    {
        var result = await mediator.Send(new GetAdminReportQuery());
        return Ok(result);
    }

    [HttpGet]
    [Authorize(AuthorizationPolicyConstants.ADMIN_POLICY)]
    public async Task<IActionResult> ExportAdminReportPdf()
    {
        var report = await mediator.Send(new GetAdminReportQuery());

        var document = new AdminReportPdfDocument(report);
        var pdfBytes = document.GeneratePdf();

        var fileName = $"admin-report-{DateTime.Now:yyyyMMdd-HHmm}.pdf";

        return File(pdfBytes, "application/pdf", fileName);
    }
}