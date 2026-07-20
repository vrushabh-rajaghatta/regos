using RegOS.Platform.Application.Queries.GetUsers;
using RegOS.Platform.Domain.Aggregates.User;

namespace RegOS.Api.Endpoints.Platform;

public static class ListUsersEndpoint
{
    public static IEndpointRouteBuilder MapListUsers(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/platform/users",
            HandleAsync)
        .WithName("ListUsers")
        .WithSummary("List users")
        .WithTags("Platform");

        return app;
    }

    private static async Task<IResult> HandleAsync(
        GetUsersHandler handler,
        CancellationToken cancellationToken,
        string? search = null,
        string? status = null,
        int page = GetUsersQuery.DefaultPage,
        int pageSize = GetUsersQuery.DefaultPageSize)
    {
        // An unparseable status is a malformed request, not an empty result.
        UserStatus? parsedStatus = null;

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<UserStatus>(status, ignoreCase: true, out var value))
                return Results.Problem(
                    detail: $"'{status}' is not a valid user status.",
                    statusCode: StatusCodes.Status400BadRequest);

            parsedStatus = value;
        }

        var result = await handler.HandleAsync(
            new GetUsersQuery(
                search,
                parsedStatus,
                page,
                pageSize),
            cancellationToken);

        return Results.Ok(result);
    }
}
