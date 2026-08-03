using RegOS.Study.Application.Commands.RegisterNonClinicalStudy;

namespace RegOS.Api.Endpoints.Studies;

public static class RegisterNonClinicalStudyEndpoint
{
    public static IEndpointRouteBuilder MapRegisterNonClinicalStudy(
        this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/studies/nonclinical", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        RegisterStudyRequest request,
        RegisterNonClinicalStudyHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new RegisterNonClinicalStudyCommand(
                request.SponsorStudyIdentifier,
                request.Title),
            cancellationToken);

        return Results.Created(
            $"/api/studies/nonclinical/{result.Id.Value}",
            new RegisterStudyResponse(result.Id.Value));
    }
}
