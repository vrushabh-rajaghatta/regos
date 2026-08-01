using RegOS.Interaction.Application.Commands.RespondToQuestion;
using RegOS.Interaction.Domain.Correspondence;

namespace RegOS.Api.Endpoints.Correspondence;

public static class RespondToQuestionEndpoint
{
    public static IEndpointRouteBuilder MapRespondToQuestion(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/correspondence/{correspondenceId:guid}/questions/{questionId:guid}/response",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid correspondenceId,
        Guid questionId,
        RespondToQuestionRequest request,
        RespondToQuestionHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new RespondToQuestionCommand(
                HaCorrespondenceId.From(correspondenceId),
                HaQuestionId.From(questionId),
                request.ResponseText,
                request.OccurredOn,
                request.Note),
            cancellationToken);

        return Results.NoContent();
    }
}
