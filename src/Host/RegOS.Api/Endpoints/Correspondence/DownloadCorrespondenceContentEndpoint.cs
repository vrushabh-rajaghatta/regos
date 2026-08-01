using RegOS.Interaction.Application.Queries.GetCorrespondenceContent;
using RegOS.Interaction.Domain.Correspondence;

namespace RegOS.Api.Endpoints.Correspondence;

public static class DownloadCorrespondenceContentEndpoint
{
    public static IEndpointRouteBuilder MapDownloadCorrespondenceContent(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/correspondence/{correspondenceId:guid}/content/{attachmentId:guid}",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid correspondenceId,
        Guid attachmentId,
        GetCorrespondenceContentHandler handler,
        CancellationToken cancellationToken)
    {
        var content = await handler.HandleAsync(
            new GetCorrespondenceContentQuery(
                HaCorrespondenceId.From(correspondenceId),
                CorrespondenceAttachmentId.From(attachmentId)),
            cancellationToken);

        // fileDownloadName preserves the name it arrived under — forwarding
        // "a1b2c3" to a colleague is not the same as forwarding
        // "FDA-IR-2019-06-14.pdf".
        return Results.File(
            content.Content,
            content.ContentType,
            content.OriginalFileName);
    }
}
