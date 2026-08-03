using RegOS.Submission.Application.Generation;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Api.Endpoints.Submissions;

public static class GenerateSequencePackageEndpoint
{
    public static IEndpointRouteBuilder MapGenerateSequencePackage(
        this IEndpointRouteBuilder app)
    {
        // POST rather than GET: generating a package is work, and a GET that
        // does minutes of it is a GET nobody can cache or retry safely.
        app.MapPost("/api/submissions/{submissionId:guid}/package", HandleAsync)
            .WithName("GenerateSequencePackage")
            .WithSummary("Generate the eCTD package for a published sequence")
            .WithTags("Submissions");

        return app;
    }

    /// <remarks>
    /// <b>No try/catch.</b> Every refusal is a semantic exception the middleware
    /// maps (ADR-012) — and each of the five says a different thing to a
    /// different person, which is the whole reason they were kept apart.
    /// </remarks>
    private static async Task<IResult> HandleAsync(
        Guid submissionId,
        SequencePackageAssembler assembler,
        CancellationToken cancellationToken)
    {
        var package = await assembler.AssembleAsync(
            new SubmissionId(submissionId), cancellationToken);

        // "Generate eCTD Package" is a permitted phrase; "validated",
        // "FDA-ready" and "ready for submission" are not, and none of them is
        // asserted here or anywhere the response reaches. Structural validity
        // is a weaker promise than it sounds (Level 2a, not 2b), and the
        // product must not imply otherwise.
        return Results.File(
            package.Contents,
            "application/zip",
            package.FileName);
    }
}
