using RegOS.ReferenceData.Application.Queries.Organization.ListContactRoles;

namespace RegOS.Api.Endpoints.ReferenceData;

public static class ListContactRolesEndpoint
{
    public static IEndpointRouteBuilder MapListContactRoles(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/reference-data/contact-roles",
            HandleAsync)
        .WithName("ListContactRoles")
        .WithSummary("List the roles a contact can hold")
        .WithTags("Reference Data");

        return app;
    }

    private static async Task<IResult> HandleAsync(
        ListContactRolesHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ListContactRolesQuery(),
            cancellationToken);

        return Results.Ok(result);
    }
}
