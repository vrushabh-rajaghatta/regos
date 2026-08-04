using RegOS.Api.Authentication;
using RegOS.Platform.Application.Commands.ActivateTenant;
using RegOS.Platform.Application.Commands.CreateTenant;
using RegOS.Platform.Application.Commands.DeactivateTenant;
using RegOS.Platform.Application.Commands.RenameTenant;
using RegOS.Platform.Application.Queries.GetTenants;
using RegOS.Platform.Application.Queries.GetTenantUsers;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Api.Endpoints.PlatformAdministration;

/// <summary>
/// Tenant administration (ADR-033): every route requires the platform
/// administrator. One file rather than one per endpoint — these five routes
/// share a policy, a tag and a lifecycle, and will grow together.
/// </summary>
public static class TenantEndpoints
{
    public static IEndpointRouteBuilder MapTenantAdministration(
        this IEndpointRouteBuilder app)
    {
        var tenants = app.MapGroup("/api/platform/tenants")
            .RequireAuthorization(RegOSPolicies.PlatformAdministrator)
            .WithTags("Tenant Administration");

        tenants.MapGet("/", ListAsync)
            .WithName("ListTenants")
            .WithSummary("Every tenant on the platform");

        tenants.MapPost("/", CreateAsync)
            .WithName("CreateTenant")
            .WithSummary(
                "Provision a customer: tenant and invited administrator");

        tenants.MapPut("/{id:guid}", RenameAsync)
            .WithName("RenameTenant");

        tenants.MapPost("/{id:guid}/activate", ActivateAsync)
            .WithName("ActivateTenant");

        tenants.MapPost("/{id:guid}/deactivate", DeactivateAsync)
            .WithName("DeactivateTenant");

        tenants.MapGet("/{id:guid}/users", ListUsersAsync)
            .WithName("ListTenantUsers")
            .WithSummary("A tenant's users, read across the tenant boundary");

        return app;
    }

    private static async Task<IResult> ListAsync(
        GetTenantsHandler handler,
        CancellationToken cancellationToken) =>
        Results.Ok(await handler.HandleAsync(cancellationToken));

    private static async Task<IResult> CreateAsync(
        CreateTenantRequest request,
        CreateTenantHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new CreateTenantCommand(
                request.Name,
                request.AdminEmail,
                request.AdminFirstName,
                request.AdminLastName),
            cancellationToken);

        return Results.Created(
            $"/api/platform/tenants/{result.TenantId.Value}",
            new CreateTenantResponse(
                result.TenantId.Value,
                result.AdminUserId.Value));
    }

    private static async Task<IResult> RenameAsync(
        Guid id,
        RenameTenantRequest request,
        RenameTenantHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new RenameTenantCommand(new TenantId(id), request.Name),
            cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> ActivateAsync(
        Guid id,
        ActivateTenantHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new ActivateTenantCommand(new TenantId(id)), cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> DeactivateAsync(
        Guid id,
        DeactivateTenantHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new DeactivateTenantCommand(new TenantId(id)), cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> ListUsersAsync(
        Guid id,
        GetTenantUsersHandler handler,
        CancellationToken cancellationToken) =>
        Results.Ok(await handler.HandleAsync(
            new TenantId(id), cancellationToken));
}

public sealed record CreateTenantRequest(
    string? Name,
    string? AdminEmail,
    string? AdminFirstName,
    string? AdminLastName);

public sealed record CreateTenantResponse(
    Guid TenantId,
    Guid AdminUserId);

public sealed record RenameTenantRequest(string? Name);
