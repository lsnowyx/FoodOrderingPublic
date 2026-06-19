namespace Application.DTOs.Common;

public sealed record PaginatedResponse<T>(
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    IReadOnlyList<T> Items)
{
    public static PaginatedResponse<T> Create(
        PaginationParameters pagination,
        int totalCount,
        IReadOnlyList<T> items)
    {
        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(totalCount / (double)pagination.PageSize);

        return new PaginatedResponse<T>(
            pagination.Page,
            pagination.PageSize,
            totalCount,
            totalPages,
            items);
    }
}
