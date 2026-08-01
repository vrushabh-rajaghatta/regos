using RegOS.Interaction.Application.Commands.AssignQuestion;
using RegOS.Interaction.Domain.Correspondence;
using RegOS.Platform.Contracts;

namespace RegOS.Api.Endpoints.Correspondence;

public static class AssignQuestionEndpoint
{
    public static IEndpointRouteBuilder MapAssignQuestion(
        this IEndpointRouteBuilder app)
    {
        app.MapPut(
            "/api/correspondence/{correspondenceId:guid}/questions/{questionId:guid}/owner",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid correspondenceId,
        Guid questionId,
        AssignQuestionRequest request,
        AssignQuestionHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new AssignQuestionCommand(
                HaCorrespondenceId.From(correspondenceId),
                HaQuestionId.From(questionId),
                request.OwnerUserId is { } owner ? UserId.From(owner) : null),
            cancellationToken);

        return Results.NoContent();
    }
}
