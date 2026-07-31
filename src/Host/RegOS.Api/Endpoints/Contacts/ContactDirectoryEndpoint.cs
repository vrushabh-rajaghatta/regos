using RegOS.Organization.Application.Queries.Contacts.ContactDirectory;
using RegOS.ReferenceData.Domain.Organization;

namespace RegOS.Api.Endpoints.Contacts;

public static class ContactDirectoryEndpoint
{
    public static IEndpointRouteBuilder MapContactDirectory(
        this IEndpointRouteBuilder app)
    {
        // "Who is the QP?" — across the tenant's whole registry.
        app.MapGet("/api/contacts", HandleAsync)
            .WithName("ContactDirectory")
            .WithSummary("Every contact in the registry, filterable by role")
            .WithTags("Contacts");

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid? roleId,
        ContactDirectoryHandler handler,
        CancellationToken cancellationToken)
        => Results.Ok(await handler.HandleAsync(
            new ContactDirectoryQuery(
                roleId is { } id ? new ContactRoleId(id) : null),
            cancellationToken));
}
