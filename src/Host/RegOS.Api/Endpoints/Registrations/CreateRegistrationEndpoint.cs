using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.Registration.Application.Commands.CreateRegistration;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

namespace RegOS.Api.Endpoints.Registrations;

public static class CreateRegistrationEndpoint
{
    public static IEndpointRouteBuilder MapCreateRegistration(
        this IEndpointRouteBuilder app)
    {
        // Scoped to the medicinal product, not the global one: a licence is
        // granted over a product in a market, and the route now names exactly
        // the thing it is granted over. The country is no longer a body field
        // because there is nothing left for the caller to decide about it.
        app.MapPost(
            "/api/medicinal-products/{medicinalProductId:guid}/registrations",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid medicinalProductId,
        CreateRegistrationRequest request,
        CreateRegistrationHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new CreateRegistrationCommand(
                new MedicinalProductId(medicinalProductId),
                new AuthorityId(request.AuthorityId),
                new OrganizationId(request.HolderOrganizationId),
                request.OccurredOn,
                request.OriginatingApplicationId is { } applicationId
                    ? new RegulatoryApplicationId(applicationId)
                    : null,
                request.Note),
            cancellationToken);

        return Results.Created(
            $"/api/registrations/{result.Id.Value}",
            new CreateRegistrationResponse(result.Id.Value));
    }
}
