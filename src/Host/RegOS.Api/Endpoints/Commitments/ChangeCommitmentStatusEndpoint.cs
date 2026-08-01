using RegOS.Interaction.Application.Commands.ChangeCommitmentStatus;
using RegOS.Interaction.Domain.Commitments;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Api.Endpoints.Commitments;

public static class ChangeCommitmentStatusEndpoint
{
    public static IEndpointRouteBuilder MapChangeCommitmentStatus(
        this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/commitments/{commitmentId:guid}/status", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid commitmentId,
        ChangeCommitmentStatusRequest request,
        ChangeCommitmentStatusHandler handler,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<CommitmentStatus>(
                request.Status, ignoreCase: true, out var target))
        {
            throw new DomainException(
                "Status must be one of InProgress, Fulfilled or Waived.");
        }

        await handler.HandleAsync(
            new ChangeCommitmentStatusCommand(
                CommitmentId.From(commitmentId),
                target,
                request.OccurredOn,
                request.Note),
            cancellationToken);

        return Results.NoContent();
    }
}
