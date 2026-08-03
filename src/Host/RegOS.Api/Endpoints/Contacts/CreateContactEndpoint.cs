using RegOS.Organization.Application.Commands.CreateContact;
using RegOS.Organization.Domain.Aggregates.Contact;
using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Organization;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Api.Endpoints.Contacts;

public static class CreateContactEndpoint
{
    public static IEndpointRouteBuilder MapCreateContact(
        this IEndpointRouteBuilder app)
    {
        // Organization-scoped: a contact always belongs to a company, even when
        // the site they work at is unknown.
        app.MapPost("/api/organizations/{organizationId:guid}/contacts", HandleAsync)
            .WithName("CreateContact")
            .WithSummary("Record a named person at an organization")
            .WithTags("Contacts");

        return app;
    }

    // No try/catch: the aggregate raises DomainException and the middleware
    // maps it to 400, the same as every other capability (ADR-012).
    private static async Task<IResult> HandleAsync(
        Guid organizationId,
        CreateContactRequest request,
        CreateContactHandler handler,
        CancellationToken cancellationToken)
    {
        var id = await handler.HandleAsync(
            new CreateContactCommand(
                new OrganizationId(organizationId),
                request.FirstName,
                request.LastName,
                request.StatusDate,
                request.OrganizationSiteId is { } siteId
                    ? new OrganizationSiteId(siteId)
                    : null,
                request.Title,
                request.Department,
                request.CountryId is { } countryId
                    ? new CountryId(countryId)
                    : null,
                [.. (request.RoleIds ?? []).Select(x => new ContactRoleId(x))],
                request.Emails,
                [.. (request.Phones ?? []).Select(PhoneOf)]),
            cancellationToken);

        return Results.Created(
            $"/api/contacts/{id.Value}",
            new CreateContactResponse(id.Value));
    }

    /// <summary>
    /// The wire sends the kind by name; an unrecognised one is a bad request,
    /// not a silent null.
    /// </summary>
    /// <remarks>
    /// Parsed here rather than bound by the serialiser so that <c>"Moblie"</c>
    /// is rejected instead of arriving as <c>null</c> and being stored as
    /// <em>nobody was asked</em>. The two nulls mean different things and only
    /// one of them is legitimate.
    /// </remarks>
    private static ContactPhoneInput PhoneOf(CreateContactPhone phone)
    {
        if (string.IsNullOrWhiteSpace(phone.Kind))
            return new ContactPhoneInput(phone.Number, null);

        if (!Enum.TryParse<ContactPhoneKind>(phone.Kind, out var kind)
            || !Enum.IsDefined(kind))
        {
            throw new DomainException(ContactErrors.PhoneKindUnknown);
        }

        return new ContactPhoneInput(phone.Number, kind);
    }
}
