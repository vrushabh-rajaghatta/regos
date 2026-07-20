namespace RegOS.Platform.Application.Common;

/// <summary>
/// The shape every paged query returns. Lives in the application layer, not the
/// shared kernel: paging is a query/transport concern, not a domain building
/// block. It is promoted only if a genuine cross-cutting need appears — several
/// modules happening to page data is duplication of a pattern, not of a concept.
/// </summary>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize);
