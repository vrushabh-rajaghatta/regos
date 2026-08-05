using RegOS.Product.Domain.Product;
using RegOS.Registration.Application.Commands.AuthorisePack;
using RegOS.Registration.Domain.Aggregates.Registration;

namespace RegOS.Api.Endpoints.Registrations;

public static class AuthorisePackEndpoint
{
    /// <remarks>
    /// Nested under the licence, because that is the gesture: <em>this licence
    /// authorises that pack</em>. The relationship is its own aggregate all the
    /// same (ADR-061 §3), so removing one is flat under
    /// <c>/api/pack-authorisations</c>.
    /// </remarks>
    public static IEndpointRouteBuilder MapAuthorisePack(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/registrations/{registrationId:guid}/authorised-packs",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid registrationId,
        AuthorisePackRequest request,
        AuthorisePackHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new AuthorisePackCommand(
                new RegistrationId(registrationId),
                PackagedProductId.From(request.PackagedProductId),
                request.AuthorisedOn),
            cancellationToken);

        return Results.Created(
            $"/api/pack-authorisations/{result.PackAuthorisationId}",
            new PackAuthorisationResponse(result.PackAuthorisationId));
    }
}
