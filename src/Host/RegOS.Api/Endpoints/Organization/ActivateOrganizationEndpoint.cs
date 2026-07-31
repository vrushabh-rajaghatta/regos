using RegOS.Organization.Application.Commands.ActivateOrganization;
using RegOS.Organization.Domain.Aggregates.Organization;

namespace RegOS.Api.Endpoints.Organization;

public static class ActivateOrganizationEndpoint
{
    public static IEndpointRouteBuilder MapActivateOrganization(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/organizations/{id:guid}/activate",
            HandleAsync)
        .WithName("ActivateOrganization")
        .WithSummary("Activate an organization")
        .WithTags("Organization");

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        ActivateOrganizationHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new ActivateOrganizationCommand(new OrganizationId(id)),
            cancellationToken);

        return Results.NoContent();
    }
}
