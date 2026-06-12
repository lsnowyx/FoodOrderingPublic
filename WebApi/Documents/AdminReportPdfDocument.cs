using Application.DTOs.Responses.Reports;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace WebApi.Documents;

public sealed class AdminReportPdfDocument : IDocument
{
    private readonly AdminReportResponse report;

    public AdminReportPdfDocument(AdminReportResponse report)
    {
        this.report = report;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(40);

            page.Header()
                .Text("Admin Requests Report")
                .SemiBold()
                .FontSize(22);

            page.Content().Column(column =>
            {
                column.Spacing(20);

                column.Item().Text($"Generated at: {DateTime.Now:dd.MM.yyyy HH:mm}")
                    .FontSize(10);

                column.Item().Row(row =>
                {
                    row.RelativeItem().Element(Card).Column(card =>
                    {
                        card.Item().Text("Total Requests").SemiBold();
                        card.Item().Text(report.TotalRequests.ToString()).FontSize(18);
                    });

                    row.RelativeItem().Element(Card).Column(card =>
                    {
                        card.Item().Text("Completed").SemiBold();
                        card.Item().Text(report.CompletedRequests.ToString()).FontSize(18);
                    });

                    row.RelativeItem().Element(Card).Column(card =>
                    {
                        card.Item().Text("Completion Rate").SemiBold();
                        card.Item().Text($"{report.CompletionPercentage}%").FontSize(18);
                    });
                });

                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).Text("Worker");
                        header.Cell().Element(HeaderCell).Text("Total Assigned");
                        header.Cell().Element(HeaderCell).Text("Completed");
                        header.Cell().Element(HeaderCell).Text("Active");
                    });

                    foreach (var worker in report.Workers)
                    {
                        table.Cell().Element(BodyCell).Text(worker.WorkerName);
                        table.Cell().Element(BodyCell).Text(worker.TotalAssignedRequests.ToString());
                        table.Cell().Element(BodyCell).Text(worker.CompletedRequests.ToString());
                        table.Cell().Element(BodyCell).Text(worker.ActiveRequests.ToString());
                    }

                    if (report.Workers.Count == 0)
                    {
                        table.Cell().ColumnSpan(4).Element(BodyCell).Text("No worker data available.");
                    }
                });
            });

            page.Footer()
                .AlignCenter()
                .Text(text =>
                {
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
        });
    }

    private static IContainer Card(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(10);
    }

    private static IContainer HeaderCell(IContainer container)
    {
        return container
            .Background(Colors.Grey.Lighten3)
            .Border(1)
            .BorderColor(Colors.Grey.Lighten1)
            .Padding(5);
    }

    private static IContainer BodyCell(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(5);
    }
}