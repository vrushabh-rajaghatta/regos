using RegOS.Process.Application.Queries.GetProcessDefinition;

namespace RegOS.Api.Endpoints.ProcessDefinitions;

public static class GetProcessDefinitionEndpoint
{
    public static IEndpointRouteBuilder MapGetProcessDefinitionEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/process-definitions/{id:guid}", HandleAsync);

        return endpoints;
    }

    // No null check and no catch: the handler raises NotFoundException and the
    // middleware maps it to 404, the same as every other capability.
    private static async Task<IResult> HandleAsync(
        Guid id,
        int? version,
        GetProcessDefinitionHandler handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(
            new GetProcessDefinitionQuery(id, version),
            cancellationToken);

        return Results.Ok(response);
    }
}
