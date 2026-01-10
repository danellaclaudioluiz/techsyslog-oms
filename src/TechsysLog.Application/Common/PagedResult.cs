namespace TechsysLog.Application.Common;

/// <summary>
/// Represents a paginated result using cursor-based pagination.
/// More efficient than offset-based for large datasets.
/// </summary>
public class PagedResult<T>
{
    public IReadOnlyList<T> Data { get; }
    public string? Cursor { get; }
    public bool HasMore { get; }
    public int Limit { get; }
    public int TotalCount { get; }

    private PagedResult(IReadOnlyList<T> data, string? cursor, bool hasMore, int limit, int totalCount)
    {
        Data = data;
        Cursor = cursor;
        HasMore = hasMore;
        Limit = limit;
        TotalCount = totalCount;
    }

    public static PagedResult<T> Create(
        IReadOnlyList<T> data,
        string? cursor,
        bool hasMore,
        int limit,
        int totalCount)
    {
        return new PagedResult<T>(data, cursor, hasMore, limit, totalCount);
    }

    public static PagedResult<T> Empty(int limit)
    {
        return new PagedResult<T>(Array.Empty<T>(), null, false, limit, 0);
    }
}