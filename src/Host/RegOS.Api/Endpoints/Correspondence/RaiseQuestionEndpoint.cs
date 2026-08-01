using RegOS.Interaction.Application.Commands.RaiseQuestion;
using RegOS.Interaction.Domain.Correspondence;

namespace RegOS.Api.Endpoints.Correspondence;

public static class RaiseQuestionEndpoint
{
    public static IEndpointRouteBuilder MapRaiseQuestion(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/correspondence/{correspondenceId:guid}/questions",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid correspondenceId,
        RaiseQuestionRequest request,
        RaiseQuestionHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new RaiseQuestionCommand(
                HaCorrespondenceId.From(correspondenceId),
                request.Number,
                request.Text,
                request.TargetResponseOn),
            cancellationToken);

        return Results.Created(
            $"/api/correspondence/{correspondenceId}/questions/{result.QuestionId.Value}",
            new RaiseQuestionResponse(result.QuestionId.Value));
    }
}
