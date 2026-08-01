using RegOS.Interaction.Application.Commands.RemoveCorrespondenceContent;
using RegOS.Interaction.Domain.Correspondence;

namespace RegOS.Api.Endpoints.Correspondence;

public static class RemoveCorrespondenceContentEndpoint
{
    public static IEndpointRouteBuilder MapRemoveCorrespondenceContent(
        this IEndpointRouteBuilder app)
    {
        app.MapDelete(
            "/api/correspondence/{correspondenceId:guid}/content/{attachmentId:guid}",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid correspondenceId,
        Guid attachmentId,
        RemoveCorrespondenceContentHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new RemoveCorrespondenceContentCommand(
                HaCorrespondenceId.From(correspondenceId),
                CorrespondenceAttachmentId.From(attachmentId)),
            cancellationToken);

        return Results.NoContent();
    }
}
