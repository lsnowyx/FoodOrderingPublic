namespace Application.DTOs.Common;

public sealed record PaginationParameters(int Page, int PageSize)
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 12;
    public const int LookupPageSize = 20;
    public const int DefaultMaxPageSize = 50;

    public int Skip => (Page - 1) * PageSize;

    public static PaginationParameters Create(
        int page,
        int pageSize,
        int maxPageSize = DefaultMaxPageSize)
    {
        var normalizedPage = Math.Max(page, 1);
        var normalizedPageSize = Math.Clamp(pageSize, 1, maxPageSize);

        return new PaginationParameters(normalizedPage, normalizedPageSize);
    }
}
