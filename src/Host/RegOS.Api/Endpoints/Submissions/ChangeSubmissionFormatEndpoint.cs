using RegOS.SharedKernel.Exceptions;
using RegOS.Submission.Application.Commands.ChangeSubmissionFormat;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Api.Endpoints.Submissions;

public static class ChangeSubmissionFormatEndpoint
{
    public static IEndpointRouteBuilder MapChangeSubmissionFormat(
        this IEndpointRouteBuilder app)
    {
        // PUT: the body states the whole value, so sending it twice lands in
        // the same place. Rejected by the aggregate once the sequence is
        // published — a filing's format is not editable after the fact
        // (ADR-047).
        app.MapPut("/api/submissions/{submissionId:guid}/format", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid submissionId,
        ChangeSubmissionFormatRequest request,
        ChangeSubmissionFormatHandler handler,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<SubmissionFormat>(
                request.Format, ignoreCase: true, out var format))
        {
            throw new DomainException(
                "Format must be one of Ectd, Nees or Paper.");
        }

        await handler.HandleAsync(
            new ChangeSubmissionFormatCommand(
                SubmissionId.From(submissionId),
                format),
            cancellationToken);

        return Results.NoContent();
    }
}
