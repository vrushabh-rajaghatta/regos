using RegOS.Interaction.Application.Queries.ListMeetings;

namespace RegOS.Api.Endpoints.Meetings;

public static class ListMeetingsEndpoint
{
    public static IEndpointRouteBuilder MapListMeetings(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/meetings", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        ListMeetingsHandler handler,
        CancellationToken cancellationToken,
        bool includeConcluded = false)
    {
        var result = await handler.HandleAsync(
            new ListMeetingsQuery(includeConcluded), cancellationToken);

        return Results.Ok(result);
    }
}
