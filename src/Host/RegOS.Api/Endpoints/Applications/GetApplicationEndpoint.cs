using RegOS.RegulatoryApplication.Application.Queries.Applications.GetApplication;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

namespace RegOS.Api.Endpoints.Applications;

public static class GetApplicationEndpoint
{
    public static IEndpointRouteBuilder MapGetApplication(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/applications/{applicationId:guid}",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid applicationId,
        GetApplicationHandler handler,
        CancellationToken cancellationToken)
    {
        var application = await handler.HandleAsync(
            new RegulatoryApplicationId(applicationId),
            cancellationToken);

        return application is null
            ? Results.NotFound()
            : Results.Ok(application);
    }
}
