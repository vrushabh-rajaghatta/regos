using RegOS.Organization.Application.Queries.Contacts.ListOrganizationContacts;
using RegOS.Organization.Domain.Aggregates.Organization;

namespace RegOS.Api.Endpoints.Contacts;

public static class ListOrganizationContactsEndpoint
{
    public static IEndpointRouteBuilder MapListOrganizationContacts(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/organizations/{organizationId:guid}/contacts", HandleAsync)
            .WithName("ListOrganizationContacts")
            .WithSummary("The people we know at one organization")
            .WithTags("Contacts");

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid organizationId,
        ListOrganizationContactsHandler handler,
        CancellationToken cancellationToken)
    {
        var contacts = await handler.HandleAsync(
            new ListOrganizationContactsQuery(new OrganizationId(organizationId)),
            cancellationToken);

        return contacts is null ? Results.NotFound() : Results.Ok(contacts);
    }
}
