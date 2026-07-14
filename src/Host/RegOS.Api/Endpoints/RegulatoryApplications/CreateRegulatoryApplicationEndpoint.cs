using RegOS.MasterData.Domain.Geography.Country;
using RegOS.MasterData.Domain.Regulatory.Authority;
using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Product.Domain.Product;
using RegOS.RegulatoryApplication.Application.Commands.CreateRegulatoryApplication;

namespace RegOS.Api.Endpoints.RegulatoryApplications;

public static class CreateRegulatoryApplicationEndpoint
{
    public static IEndpointRouteBuilder MapCreateRegulatoryApplication(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/products/{productId:guid}/applications",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid productId,
        CreateRegulatoryApplicationRequest request,
        CreateRegulatoryApplicationHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new CreateRegulatoryApplicationCommand(
                new ProductId(productId),
                new CountryId(request.CountryId),
                new AuthorityId(request.AuthorityId),
                new OrganizationId(request.ApplicantOrganizationId),
                request.Name),
            cancellationToken);

        return Results.Created(
            $"/api/applications/{result.Id}",
            new CreateRegulatoryApplicationResponse(
                result.Id.Value));
    }
}
