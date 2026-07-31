using RegOS.Organization.Application.Commands.AddOrganizationIdentifier;
using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.ReferenceData.Domain.Organization;

namespace RegOS.Api.Endpoints.Organization;

public static class AddOrganizationIdentifierEndpoint
{
    public static IEndpointRouteBuilder MapAddOrganizationIdentifier(
        this IEndpointRouteBuilder app)
    {
        // Its own endpoint rather than a field on PUT /organizations/{id}: an
        // identifier is issued by a registry, not corrected like a name, and a
        // form submit that dropped the array would erase every one the company
        // holds.
        app.MapPost(
            "/api/organizations/{organizationId:guid}/identifiers",
            HandleAsync)
        .WithName("AddOrganizationIdentifier")
        .WithSummary("Record an identifier issued to an organization")
        .WithTags("Organization");

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid organizationId,
        AddOrganizationIdentifierRequest request,
        AddOrganizationIdentifierHandler handler,
        CancellationToken cancellationToken)
    {
        var identifierId = await handler.HandleAsync(
            new AddOrganizationIdentifierCommand(
                new OrganizationId(organizationId),
                new IdentifierSchemeId(request.SchemeId),
                request.Value),
            cancellationToken);

        return Results.Created(
            $"/api/organizations/{organizationId}/identifiers/{identifierId.Value}",
            new AddOrganizationIdentifierResponse(identifierId.Value));
    }
}

public sealed record AddOrganizationIdentifierRequest(
    Guid SchemeId,
    string Value);

public sealed record AddOrganizationIdentifierResponse(Guid Id);
