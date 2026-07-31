using RegOS.Organization.Application.Commands.CreateOrganizationDivision;
using RegOS.Organization.Domain.Aggregates.Organization;

namespace RegOS.Api.Endpoints.OrganizationDivisions;

public static class CreateOrganizationDivisionEndpoint
{
    public static IEndpointRouteBuilder MapCreateOrganizationDivision(
        this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/organizations/{organizationId:guid}/divisions", HandleAsync)
            .WithName("CreateOrganizationDivision")
            .WithSummary("Record a business unit within an organization")
            .WithTags("Organization Divisions");

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid organizationId,
        CreateOrganizationDivisionRequest request,
        CreateOrganizationDivisionHandler handler,
        CancellationToken cancellationToken)
    {
        var id = await handler.HandleAsync(
            new CreateOrganizationDivisionCommand(
                new OrganizationId(organizationId),
                request.Name,
                request.StatusDate,
                request.Acronym),
            cancellationToken);

        return Results.Created(
            $"/api/organizations/{organizationId}/divisions",
            new CreateOrganizationDivisionResponse(id.Value));
    }
}

/// <param name="StatusDate">The business date the division was established.</param>
public sealed record CreateOrganizationDivisionRequest(
    string Name,
    DateOnly StatusDate,
    string? Acronym = null);

public sealed record CreateOrganizationDivisionResponse(Guid Id);
