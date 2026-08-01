using RegOS.Interaction.Application.Commands.BeginInspection;
using RegOS.Interaction.Domain.Inspections;
using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Api.Endpoints.Inspections;

public static class BeginInspectionEndpoint
{
    public static IEndpointRouteBuilder MapBeginInspection(
        this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/inspections", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        BeginInspectionRequest request,
        BeginInspectionHandler handler,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<InspectionStatus>(
                request.InitialStatus, ignoreCase: true, out var initial))
        {
            throw new DomainException(
                "An inspection begins either Announced or InProgress.");
        }

        var result = await handler.HandleAsync(
            new BeginInspectionCommand(
                new AuthorityId(request.AuthorityId),
                request.Title,
                initial,
                request.OccurredOn,
                request.OrganizationSiteId is { } site
                    ? OrganizationSiteId.From(site)
                    : null,
                request.ScheduledFor),
            cancellationToken);

        return Results.Created(
            $"/api/inspections/{result.InspectionId.Value}",
            new BeginInspectionResponse(result.InspectionId.Value));
    }
}
