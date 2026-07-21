using RegOS.Platform.Domain.Aggregates.User;

namespace RegOS.Persistence.ReadModels;

/// <summary>
/// A flat, read-only row over the Users table for the user directory.
/// </summary>
/// <remarks>
/// Deliberately bypasses the User aggregate and its value converters. Reading
/// through the write model is fine for projections but breaks down in
/// predicates: `user.Email.Value` cannot be translated to SQL, and
/// `EF.Property&lt;string&gt;` still runs the Email converter over the parameter.
/// A keyless row of primitives keeps searching, filtering, sorting and paging
/// fully translatable — reporting, not domain modelling.
/// </remarks>
public sealed class UserDirectoryRow
{
    public Guid Id { get; init; }

    // Null for a platform user. A directory query comparing this against a
    // caller's tenant can never match null, so platform users are invisible
    // in every tenant's directory without any code hiding them.
    public Guid? TenantId { get; init; }

    public string FirstName { get; init; } = default!;

    public string LastName { get; init; } = default!;

    public string Email { get; init; } = default!;

    public UserStatus Status { get; init; }

    public DateTime CreatedOn { get; init; }
}
