using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.Registration.Application.Commands.CreateRegistration;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

namespace RegOS.Api.Endpoints.Registrations;

public static class CreateRegistrationEndpoint
{
    public static IEndpointRouteBuilder MapCreateRegistration(
        this IEndpointRouteBuilder app)
    {
        // Product-scoped, like applications: a registration is always *for* a
        // product, and the route says so.
        app.MapPost(
            "/api/products/{productId:guid}/registrations",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid productId,
        CreateRegistrationRequest request,
        CreateRegistrationHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new CreateRegistrationCommand(
                new ProductId(productId),
                new CountryId(request.CountryId),
                new AuthorityId(request.AuthorityId),
                new OrganizationId(request.HolderOrganizationId),
                request.OccurredOn,
                request.OriginatingApplicationId is { } applicationId
                    ? new RegulatoryApplicationId(applicationId)
                    : null,
                request.Note),
            cancellationToken);

        return Results.Created(
            $"/registrations/{result.Id.Value}",
            new CreateRegistrationResponse(result.Id.Value));
    }
}
