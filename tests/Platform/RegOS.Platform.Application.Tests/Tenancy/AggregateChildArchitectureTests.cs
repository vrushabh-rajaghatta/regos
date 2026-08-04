using System.Reflection;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;

namespace RegOS.Platform.Application.Tests.Tenancy;

/// <summary>
/// Architecture tests over the EF model itself, for two traps that have each
/// cost a build cycle in consecutive epics and are invisible in the domain
/// model: <b>a shadow foreign key is nullable by default</b>, and <b>an owned
/// value object cannot be bound to a constructor parameter</b>.
/// </summary>
/// <remarks>
/// These live beside <see cref="TenantFilterArchitectureTests"/> rather than in
/// <c>tests/Architecture/</c> because those tests deliberately reference no
/// production project — they read source text, so a convention reaches a
/// context they have never heard of. These need the built model, and the model
/// is only available where the DbContext is.
/// <para>
/// They exist instead of a checklist. A checklist depends on somebody reading
/// it, and EPIC-010a's retro recorded both of these before EPIC-018 hit the
/// second one anyway.
/// </para>
/// </remarks>
public sealed class AggregateChildArchitectureTests
{
    // Model building needs a provider but no database; nothing here connects.
    private static RegOSDbContext BareContext() =>
        new(new DbContextOptionsBuilder<RegOSDbContext>()
            .UseNpgsql("Host=localhost;Database=model-only")
            .Options);

    /// <summary>
    /// Children that predate this rule. <b>Shrink-only</b> — a new entry to
    /// unblock new code defeats the mechanism entirely.
    /// </summary>
    /// <remarks>
    /// All five are Organization's, from EPIC-016, and all five are the same
    /// mistake: a collection declared from the parent with
    /// <c>HasForeignKey("&lt;Parent&gt;Id")</c> and no <c>IsRequired()</c>.
    /// Retiring them is one line per configuration plus a migration setting five
    /// columns <c>NOT NULL</c> — which is a schema change to a shipped table and
    /// therefore its own decision, not something this test's arrival should make
    /// for somebody. Recorded in BACKLOG's standing-debt table (2026-08-04).
    /// <para>
    /// <b>Removing an entry means writing the migration, not deleting the
    /// string.</b> <c>NULL → NOT NULL</c> is a behavioural change: it fails
    /// against data that already holds a null, so retiring one of these is a
    /// review of the existing rows as much as of the configuration. The
    /// companion test below enforces the ordering — an entry cannot stay here
    /// once fixed — but nothing can enforce that the fix was considered, which
    /// is why it is written down.
    /// </para>
    /// <para>
    /// <c>ContactRoleAssignment</c> is not equal to the other four.
    /// Its unique index on <c>(ContactId, RoleId)</c> reads as <em>one role per
    /// contact</em> and does not hold, because Postgres treats NULLs as
    /// distinct. That is an integrity gap; the rest are orphans nobody can
    /// currently create.
    /// </para>
    /// </remarks>
    private static readonly string[] Grandfathered =
    [
        "ContactEmail.ContactId",
        "ContactPhone.ContactId",
        "ContactRoleAssignment.ContactId",
        "OrganizationIdentifier.OrganizationId",
        "SiteIdentifier.OrganizationSiteId"
    ];

    /// <summary>
    /// A child that cascades from its parent must be <b>required</b>.
    /// </summary>
    /// <remarks>
    /// Two things go wrong when it is not, and neither announces itself:
    /// <list type="number">
    /// <item>An orphan becomes representable — a row whose parent is null is a
    /// state no aggregate can produce and no query expects.</item>
    /// <item><b>Postgres treats NULLs as distinct</b>, so a unique index naming
    /// that column stops constraining the parentless rows. The index reads as
    /// enforcing "one per parent" and does not — which is exactly what
    /// <c>ContactRoleAssignment</c>'s unique index on
    /// <c>(ContactId, RoleId)</c> does today.</item>
    /// </list>
    /// EF infers this shape whenever a relationship is declared only from the
    /// parent side with <c>HasForeignKey("&lt;Parent&gt;Id")</c>: the shadow
    /// property it creates is nullable. Declaring it explicitly with
    /// <c>IsRequired()</c> is the fix, and <c>TradeNameConfiguration</c> is the
    /// reference.
    /// </remarks>
    [Fact]
    public void Every_cascading_child_has_a_required_foreign_key()
    {
        using var context = BareContext();

        var severable = SeverableChildren(context);

        severable.Except(Grandfathered).Should().BeEmpty(
            "a cascading child with a nullable foreign key can be orphaned, and "
            + "any unique index naming that column silently stops constraining "
            + "the orphans. Declare the shadow property with IsRequired() on the "
            + "child's configuration — see TradeNameConfiguration.");
    }

    /// <summary>
    /// The companion check: the list above may not describe rows that are
    /// already fixed, so it cannot quietly stop matching reality.
    /// </summary>
    [Fact]
    public void The_grandfathered_list_has_not_gone_stale()
    {
        using var context = BareContext();

        var severable = SeverableChildren(context);

        Grandfathered.Except(severable).Should().BeEmpty(
            "these are now required, so they belong out of the list rather than "
            + "in it. The list is shrink-only in both directions: it may not "
            + "grow, and it may not keep naming something that has been fixed.");
    }

    /// <summary>
    /// An aggregate that owns a value object must be constructible without it.
    /// </summary>
    /// <remarks>
    /// EF binds constructor parameters by name from <em>mapped properties</em>,
    /// and an owned type is not one — it cannot be bound to a parameter at all.
    /// Two shapes satisfy this and both are in use: a parameterless constructor
    /// with an object-initializer factory (<c>GlobalLabel</c>,
    /// <c>PharmaceuticalProductDetail</c>), or a parameterized constructor that
    /// takes only scalars and ids and sets the owned value afterwards
    /// (<c>HaCorrespondence</c>).
    /// <para>
    /// The failure without this is loud but unhelpful — a design-time "no
    /// suitable constructor was found" naming a parameter. This names the
    /// aggregate and says what to do about it.
    /// </para>
    /// </remarks>
    [Fact]
    public void Every_owner_of_a_value_object_can_be_constructed_without_it()
    {
        using var context = BareContext();

        var unconstructable = context.Model.GetEntityTypes()
            .Where(entity => !entity.IsOwned())
            .Select(entity => new
            {
                entity.ClrType,
                Owned = entity.GetNavigations()
                    .Where(n => n.TargetEntityType.IsOwned())
                    .Select(n => n.Name)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase)
            })
            .Where(x => x.Owned.Count > 0)
            .Where(x => !x.ClrType
                .GetConstructors(
                    BindingFlags.Instance
                    | BindingFlags.Public
                    | BindingFlags.NonPublic)
                .Any(constructor => constructor
                    .GetParameters()
                    .All(parameter => !x.Owned.Contains(parameter.Name ?? ""))))
            .Select(x => x.ClrType.Name)
            .ToList();

        unconstructable.Should().BeEmpty(
            "EF binds constructor parameters from mapped properties, and an "
            + "owned value object is not one. Either take only scalars in the "
            + "constructor and set the owned value afterwards, or use a private "
            + "parameterless constructor with an object-initializer factory.");
    }

    private static List<string> SeverableChildren(RegOSDbContext context)
        => context.Model.GetEntityTypes()
            .SelectMany(entity => entity.GetForeignKeys())
            .Where(fk => fk.DeleteBehavior == DeleteBehavior.Cascade)
            .Where(fk => !fk.IsRequired)
            .Select(fk =>
                $"{fk.DeclaringEntityType.ClrType.Name}."
                + string.Join("+", fk.Properties.Select(p => p.Name)))
            .Distinct()
            .Order()
            .ToList();
}
