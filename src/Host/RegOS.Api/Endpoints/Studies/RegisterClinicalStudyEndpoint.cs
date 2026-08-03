using RegOS.Study.Application.Commands.RegisterClinicalStudy;

namespace RegOS.Api.Endpoints.Studies;

public static class RegisterClinicalStudyEndpoint
{
    public static IEndpointRouteBuilder MapRegisterClinicalStudy(
        this IEndpointRouteBuilder app)
    {
        // Two routes, not one with a `kind` field: they create different
        // aggregates, and a discriminator on the wire is the first place a
        // discriminator in the domain would come from (ADR-056 §2).
        app.MapPost("/api/studies/clinical", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        RegisterStudyRequest request,
        RegisterClinicalStudyHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new RegisterClinicalStudyCommand(
                request.SponsorStudyIdentifier,
                request.Title),
            cancellationToken);

        return Results.Created(
            $"/api/studies/clinical/{result.Id.Value}",
            new RegisterStudyResponse(result.Id.Value));
    }
}
