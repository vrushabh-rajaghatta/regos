using RegOS.ReferenceData.Application.Commands.CreateSubstance;

namespace RegOS.Api.Endpoints.Substances;

public static class CreateSubstanceEndpoint
{
    public static IEndpointRouteBuilder MapCreateSubstance(
        this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/substances", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        CreateSubstanceRequest request,
        CreateSubstanceHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new CreateSubstanceCommand(
                request.Name,
                request.Inn,
                request.SubstanceClassCode,
                request.SubstanceTypeCode,
                request.CasNumber,
                request.UniiCode,
                request.MolecularFormula,
                request.Description),
            cancellationToken);

        return Results.Created(
            $"/api/substances/{result.Id.Value}",
            new CreateSubstanceResponse(result.Id.Value));
    }
}
