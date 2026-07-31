using RegOS.Organization.Application.Commands.UpdateOrganization;
using RegOS.Organization.Domain.Aggregates.Organization;

namespace RegOS.Api.Endpoints.Organization;

public static class UpdateOrganizationEndpoint
{
    public static IEndpointRouteBuilder MapUpdateOrganization(
        this IEndpointRouteBuilder app)
    {
        app.MapPut(
            "/api/organizations/{id:guid}",
            HandleAsync)
        .WithName("UpdateOrganization")
        .WithSummary("Update an organization")
        .WithTags("Organization");

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        UpdateOrganizationRequest request,
        UpdateOrganizationHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new UpdateOrganizationCommand(
                new OrganizationId(id),
                request.LegalName,
                request.Type),
            cancellationToken);

        return Results.NoContent();
    }
}
