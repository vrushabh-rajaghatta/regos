using RegOS.ReferenceData.Application.Queries.Blueprint.GetRegulatoryTemplate;

namespace RegOS.Api.Endpoints.ReferenceData;

public static class GetRegulatoryTemplateEndpoint
{
    public static IEndpointRouteBuilder MapGetRegulatoryTemplate(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/reference-data/templates/{id:guid}",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        GetRegulatoryTemplateHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(id, cancellationToken);

        return result is null
            ? Results.NotFound()
            : Results.Ok(result);
    }
}
