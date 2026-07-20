using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Platform.Application.Commands.UpdateUserProfile;
using RegOS.Platform.Domain.Aggregates.User;

namespace RegOS.Api.Endpoints.Platform;

public static class UpdateUserProfileEndpoint
{
    public static IEndpointRouteBuilder MapUpdateUserProfile(
        this IEndpointRouteBuilder app)
    {
        app.MapPut(
            "/api/platform/users/{id:guid}",
            HandleAsync)
        .WithName("UpdateUserProfile")
        .WithSummary("Update a user's profile")
        .WithTags("Platform");

        return app;
    }

    // 204 No Content: the client already knows what it sent. If it wants the
    // refreshed record it can GET the user.
    private static async Task<IResult> HandleAsync(
        Guid id,
        UpdateUserProfileRequest request,
        UpdateUserProfileHandler handler,
        CancellationToken cancellationToken,
        Guid? organizationId = null)
    {
        await handler.HandleAsync(
            new UpdateUserProfileCommand(
                UserId.From(id),
                request.FirstName,
                request.LastName,
                request.Email,
                organizationId is null
                    ? null
                    : new OrganizationId(organizationId.Value)),
            cancellationToken);

        return Results.NoContent();
    }
}
