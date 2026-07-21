using RegOS.Api.Authentication;
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
        .WithTags("Platform")
        // User administration belongs to the tenant administrator
        // (ADR-033): a Member is refused with 403, and a platform
        // administrator has no tenant to administer users in.
        .RequireAuthorization(RegOSPolicies.TenantAdministrator);

        return app;
    }

    // 204 No Content: the client already knows what it sent. If it wants the
    // refreshed record it can GET the user.
    private static async Task<IResult> HandleAsync(
        Guid id,
        UpdateUserProfileRequest request,
        UpdateUserProfileHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new UpdateUserProfileCommand(
                UserId.From(id),
                request.FirstName,
                request.LastName,
                request.Email),
            cancellationToken);

        return Results.NoContent();
    }
}
