using RegOS.Registration.Application.Commands.WithdrawPackAuthorisation;
using RegOS.Registration.Domain.Aggregates.PackAuthorisations;

namespace RegOS.Api.Endpoints.Registrations;

public static class WithdrawPackAuthorisationEndpoint
{
    public static IEndpointRouteBuilder MapWithdrawPackAuthorisation(
        this IEndpointRouteBuilder app)
    {
        app.MapDelete(
            "/api/pack-authorisations/{packAuthorisationId:guid}", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid packAuthorisationId,
        WithdrawPackAuthorisationHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new WithdrawPackAuthorisationCommand(
                PackAuthorisationId.From(packAuthorisationId)),
            cancellationToken);

        return Results.NoContent();
    }
}
