using RegOS.Organization.Application.Commands.DeactivateOrganization;
using RegOS.Organization.Domain.Aggregates.Organization;

namespace RegOS.Api.Endpoints.Organization;

public static class DeactivateOrganizationEndpoint
{
    public static IEndpointRouteBuilder MapDeactivateOrganization(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/organizations/{id:guid}/deactivate",
            HandleAsync)
        .WithName("DeactivateOrganization")
        .WithSummary("Deactivate an organization")
        .WithTags("Organization");

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        DeactivateOrganizationHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new DeactivateOrganizationCommand(new OrganizationId(id)),
            cancellationToken);

        return Results.NoContent();
    }
}
