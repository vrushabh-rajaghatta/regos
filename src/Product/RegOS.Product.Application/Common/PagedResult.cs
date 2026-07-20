namespace RegOS.Product.Application.Common;

/// <summary>
/// The shape every paged query in this context returns.
/// </summary>
/// <remarks>
/// Deliberately duplicated from Platform rather than shared. This is the second
/// occurrence of the pattern, and the Rule of Three says to wait: promoting it
/// now would either couple Product to Platform's application layer or put a
/// transport concern into the shared kernel, which we decided against when the
/// type was first written. On the third occurrence, promote it.
/// </remarks>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize);
