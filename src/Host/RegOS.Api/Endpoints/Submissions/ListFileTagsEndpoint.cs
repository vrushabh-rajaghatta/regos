using RegOS.Submission.Application.StudyTagging;

namespace RegOS.Api.Endpoints.Submissions;

/// <summary>
/// ICH's published <c>file-tag</c> vocabulary, for the picker that sets one.
/// </summary>
/// <remarks>
/// Served from a table in code rather than from reference data, because it is a
/// wire vocabulary nobody curates — see <c>FileTagVocabulary</c>. Not under
/// <c>/reference-data</c> for the same reason: nothing here is seeded, and a
/// route implying otherwise would invite someone to look for the table.
/// </remarks>
public static class ListFileTagsEndpoint
{
    public static IEndpointRouteBuilder MapListFileTags(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/study-tagging/file-tags", HandleAsync);

        return app;
    }

    private static IResult HandleAsync()
        => Results.Ok(FileTagVocabulary.AsMap
            .Select(entry => new FileTagOption(entry.Key, entry.Value))
            .OrderBy(tag => tag.Realm, StringComparer.Ordinal)
            .ThenBy(tag => tag.Name, StringComparer.Ordinal)
            .ToList());
}

/// <param name="Realm">
/// The <c>info-type</c> it is published under — <c>ich</c>, <c>us</c> or
/// <c>jp</c>. Surfaced so a filer can see that a tag is regional before
/// choosing it.
/// </param>
public sealed record FileTagOption(string Name, string Realm);
