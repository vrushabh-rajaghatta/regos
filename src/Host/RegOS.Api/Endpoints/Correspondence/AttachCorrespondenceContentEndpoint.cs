using RegOS.Interaction.Application.Commands.AttachCorrespondenceContent;
using RegOS.Interaction.Domain.Correspondence;

namespace RegOS.Api.Endpoints.Correspondence;

public static class AttachCorrespondenceContentEndpoint
{
    public static IEndpointRouteBuilder MapAttachCorrespondenceContent(
        this IEndpointRouteBuilder app)
    {
        // "content", not "documents": the route says what this is — the letter's
        // own content — rather than borrowing a word that means a governed
        // business object elsewhere in RegOS (ADR-040 decision 5).
        app.MapPost(
                "/api/correspondence/{correspondenceId:guid}/content",
                HandleAsync)
            // API upload consumed by our SPA; no browser antiforgery token.
            .DisableAntiforgery();

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid correspondenceId,
        IFormFile file,
        AttachCorrespondenceContentHandler handler,
        CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();

        var result = await handler.HandleAsync(
            new AttachCorrespondenceContentCommand(
                HaCorrespondenceId.From(correspondenceId),
                file.FileName,
                file.ContentType,
                stream),
            cancellationToken);

        return Results.Created(
            $"/api/correspondence/{correspondenceId}/content/{result.AttachmentId.Value}",
            new AttachCorrespondenceContentResponse(result.AttachmentId.Value));
    }
}
