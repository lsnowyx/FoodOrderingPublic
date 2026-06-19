namespace AdminPanel.Models.Common;

public class PaginatedViewModel<T>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public List<T> Items { get; set; } = new();

    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => TotalPages > Page;
}
