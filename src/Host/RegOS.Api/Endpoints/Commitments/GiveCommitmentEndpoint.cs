using RegOS.Interaction.Application.Commands.GiveCommitment;
using RegOS.Interaction.Domain.Correspondence;
using RegOS.Platform.Contracts;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.Registration.Domain.Aggregates.Registration;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

namespace RegOS.Api.Endpoints.Commitments;

public static class GiveCommitmentEndpoint
{
    public static IEndpointRouteBuilder MapGiveCommitment(
        this IEndpointRouteBuilder app)
    {
        // Not nested under correspondence: the archetype is a post-marketing
        // commitment from an approval letter, and it outlives that letter by
        // years. A route naming the source would make standalone ones
        // second-class.
        app.MapPost("/api/commitments", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        GiveCommitmentRequest request,
        GiveCommitmentHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new GiveCommitmentCommand(
                new AuthorityId(request.AuthorityId),
                request.Title,
                request.GivenOn,
                request.DueOn,
                request.Description,
                request.OwnerUserId is { } owner ? UserId.From(owner) : null,
                request.RegistrationId is { } registration
                    ? new RegistrationId(registration)
                    : null,
                request.RegulatoryApplicationId is { } application
                    ? new RegulatoryApplicationId(application)
                    : null,
                request.SourceCorrespondenceId is { } source
                    ? HaCorrespondenceId.From(source)
                    : null),
            cancellationToken);

        return Results.Created(
            $"/api/commitments/{result.CommitmentId.Value}",
            new GiveCommitmentResponse(result.CommitmentId.Value));
    }
}
