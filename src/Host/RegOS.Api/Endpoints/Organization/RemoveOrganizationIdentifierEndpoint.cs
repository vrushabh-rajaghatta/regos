using RegOS.Organization.Application.Commands.RemoveOrganizationIdentifier;
using RegOS.Organization.Domain.Aggregates.Organization;

namespace RegOS.Api.Endpoints.Organization;

public static class RemoveOrganizationIdentifierEndpoint
{
    public static IEndpointRouteBuilder MapRemoveOrganizationIdentifier(
        this IEndpointRouteBuilder app)
    {
        app.MapDelete(
            "/api/organizations/{organizationId:guid}/identifiers/{identifierId:guid}",
            HandleAsync)
        .WithName("RemoveOrganizationIdentifier")
        .WithSummary("Withdraw an identifier from an organization")
        .WithTags("Organization");

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid organizationId,
        Guid identifierId,
        RemoveOrganizationIdentifierHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new RemoveOrganizationIdentifierCommand(
                new OrganizationId(organizationId),
                new OrganizationIdentifierId(identifierId)),
            cancellationToken);

        return Results.NoContent();
    }
}
