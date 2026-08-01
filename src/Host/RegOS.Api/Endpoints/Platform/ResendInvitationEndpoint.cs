using RegOS.Api.Authentication;
using RegOS.Platform.Application.Commands.ResendInvitation;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Contracts;

namespace RegOS.Api.Endpoints.Platform;

public static class ResendInvitationEndpoint
{
    public static IEndpointRouteBuilder MapResendInvitation(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/platform/users/{userId:guid}/invitations",
            HandleAsync)
        // User administration belongs to the tenant administrator
        // (ADR-033): a Member is refused with 403, and a platform
        // administrator has no tenant to administer users in.
        .RequireAuthorization(RegOSPolicies.TenantAdministrator);

        return app;
    }

    // POST to the collection: resending creates a new invitation rather than
    // altering the old one, which is revoked. 204 rather than 201 because the
    // resource it created is a secret the caller never sees.
    private static async Task<IResult> HandleAsync(
        Guid userId,
        ResendInvitationHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new ResendInvitationCommand(UserId.From(userId)),
            cancellationToken);

        return Results.NoContent();
    }
}
