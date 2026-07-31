using FluentAssertions;

namespace RegOS.Architecture.Tests;

/// <summary>
/// SC-002 — a repository interface belongs to the aggregate, so it lives in
/// the Domain project.
///
/// This is not a new rule. ADR-016 stated it in July 2026 and five contexts
/// follow it; it is here because stating it was not enough — a repository
/// interface was added to an Application project after the ADR was written,
/// by copying the nearest neighbour instead of reading the decision.
/// </summary>
public sealed class PersistenceConventionTests
{
    /// <summary>
    /// Interfaces that predate the rule being enforced. Shrink, never grow.
    /// Moving one is a namespace change and a handful of usings — cheap, but
    /// it touches command handlers, so it happens per context.
    /// </summary>
    private static readonly HashSet<string> Grandfathered =
    [
        "src/Product/RegOS.Product.Application/Persistence/IProductRepository.cs",
        "src/Organization/RegOS.Organization.Application/Persistence/IOrganizationRepository.cs"
    ];

    [Fact]
    public void Repository_interfaces_live_in_the_domain_project()
    {
        var offenders = Repo.SourceFiles("src")
            .Where(path => IsInterfaceFile(Path.GetFileName(path)))
            .Select(Repo.Relative)
            .Where(path => !path.Contains(".Domain/", StringComparison.Ordinal))
            .Where(path => !Grandfathered.Contains(path))
            .ToList();

        offenders.Should().BeEmpty(
            "a repository interface is part of the aggregate's contract and "
            + "belongs in its Domain project (ADR-016, slice-conventions.md "
            + "SC-002). The implementation stays in Infrastructure");
    }

    [Fact]
    public void The_grandfathered_list_contains_no_stale_entries()
    {
        var stale = Grandfathered
            .Where(path => !File.Exists(Path.Combine(Repo.Root, path)))
            .ToList();

        stale.Should().BeEmpty(
            "these interfaces are exempted but no longer at that path — they "
            + "were moved or renamed, so delete them from "
            + "PersistenceConventionTests.Grandfathered");
    }

    /// <summary>
    /// The .NET interface convention: I followed by an upper-case letter.
    /// Testing only for a leading I would catch InvitationRepository, which is
    /// a class and belongs exactly where it is.
    /// </summary>
    private static bool IsInterfaceFile(string fileName) =>
        fileName.Length > 1
        && fileName[0] == 'I'
        && char.IsUpper(fileName[1])
        && fileName.EndsWith("Repository.cs", StringComparison.Ordinal);
}
