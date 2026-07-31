using RegOS.Organization.Application.Commands.CreateOrganization;

namespace RegOS.Api.Endpoints.Organization;

public static class CreateOrganizationEndpoint
{
    public static IEndpointRouteBuilder MapCreateOrganization(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/organizations",
            HandleAsync)
        .WithName("CreateOrganization")
        .WithSummary("Create an organization")
        .WithTags("Organization");

        return app;
    }

    // No try/catch: the aggregate raises DomainException and the middleware
    // maps it to 400, the same as every other capability (ADR-012).
    private static async Task<IResult> HandleAsync(
        CreateOrganizationRequest request,
        CreateOrganizationHandler handler,
        CancellationToken cancellationToken)
    {
        var organizationId = await handler.HandleAsync(
            new CreateOrganizationCommand(request.LegalName, request.Type),
            cancellationToken);

        return Results.Created(
            $"/api/organizations/{organizationId.Value}",
            new CreateOrganizationResponse(organizationId.Value));
    }
}
