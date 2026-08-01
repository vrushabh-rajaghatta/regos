using RegOS.Interaction.Application.Commands.ResolveQuestion;
using RegOS.Interaction.Domain.Correspondence;

namespace RegOS.Api.Endpoints.Correspondence;

public static class ResolveQuestionEndpoint
{
    public static IEndpointRouteBuilder MapResolveQuestion(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/correspondence/{correspondenceId:guid}/questions/{questionId:guid}/resolution",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid correspondenceId,
        Guid questionId,
        ResolveQuestionRequest request,
        ResolveQuestionHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new ResolveQuestionCommand(
                HaCorrespondenceId.From(correspondenceId),
                HaQuestionId.From(questionId),
                request.OccurredOn,
                request.Note),
            cancellationToken);

        return Results.NoContent();
    }
}
