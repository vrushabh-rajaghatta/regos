using RegOS.Organization.Application.Queries.Contacts.GetContact;
using RegOS.Organization.Domain.Aggregates.Contact;

namespace RegOS.Api.Endpoints.Contacts;

public static class GetContactEndpoint
{
    public static IEndpointRouteBuilder MapGetContact(
        this IEndpointRouteBuilder app)
    {
        // Flat and canonical, like a site: a contact is a root, and has one URL
        // whichever direction reached it.
        app.MapGet("/api/contacts/{contactId:guid}", HandleAsync)
            .WithName("GetContact")
            .WithSummary("A single contact")
            .WithTags("Contacts");

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid contactId,
        GetContactHandler handler,
        CancellationToken cancellationToken)
    {
        var contact = await handler.HandleAsync(
            new GetContactQuery(new ContactId(contactId)), cancellationToken);

        return contact is null ? Results.NotFound() : Results.Ok(contact);
    }
}
