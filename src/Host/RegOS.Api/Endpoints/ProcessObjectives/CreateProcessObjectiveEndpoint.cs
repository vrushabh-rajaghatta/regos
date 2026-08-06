using RegOS.Platform.Contracts;
using RegOS.Process.Application.Commands.CreateProcessObjective;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Geography.Country;

namespace RegOS.Api.Endpoints.ProcessObjectives;

public static class CreateProcessObjectiveEndpoint
{
    public static IEndpointRouteBuilder MapCreateProcessObjectiveEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/process-objectives", HandleAsync);

        return endpoints;
    }

    /// <summary>
    /// The request carries no status. A new objective is always Proposed, and
    /// there is deliberately no way to create one already Active — deciding to
    /// pursue something is a second, dated event (ADR-065 decision 3).
    /// </summary>
    private static async Task<IResult> HandleAsync(
        CreateProcessObjectiveRequest request,
        CreateProcessObjectiveHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new CreateProcessObjectiveCommand(
                new GlobalProductId(request.GlobalProductId),
                new CountryId(request.CountryId),
                request.Name,
                request.StatedOn,
                request.Rationale,
                request.OwnerUserId is { } owner ? new UserId(owner) : null,
                request.TargetCompletionOn),
            cancellationToken);

        return Results.Created($"/api/process-objectives/{result.Id}", result);
    }

    public sealed record CreateProcessObjectiveRequest(
        Guid GlobalProductId,
        Guid CountryId,
        string Name,
        DateOnly StatedOn,
        string? Rationale,
        Guid? OwnerUserId,
        DateOnly? TargetCompletionOn);
}
