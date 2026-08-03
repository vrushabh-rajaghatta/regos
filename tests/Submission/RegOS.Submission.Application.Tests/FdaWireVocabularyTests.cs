using System.Xml.Linq;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Submission.Application.Tests.Fixtures;

namespace RegOS.Submission.Application.Tests;

/// <summary>
/// <b>Level 2a applied to RegOS's own reference data.</b> Every FDA wire token
/// this installation would write into <c>us-regional.xml</c> is checked against
/// the list FDA publishes — [`application-type.xml`](../../../docs/evidence/EPIC-007a/spec/application-type.xml),
/// held since 2026-08-03 (evidence <b>E30</b>).
/// </summary>
/// <remarks>
/// <b>Why this can exist at all.</b> The regional DTD types `application-type`
/// as <c>CDATA</c> (E12), so no parser will ever reject <c>fdaat99</c>. The
/// vocabulary is Level 3 and unverifiable by the oracle the rest of this epic
/// leans on — until FDA publishes the enumeration as a file, at which point the
/// check becomes ours to run. It is the same move S005 made for element names
/// against the DTD.
/// <para>
/// <b>Why it reads the database rather than the seed constant.</b> A seed is
/// what a fresh clone gets; the tokens that will actually be written are the
/// rows that exist. S002 and S003 both found those two diverging, and the
/// reconciliation in <c>ApplicationTypeDataInitializer</c> is exactly the code
/// this would catch if it stopped working.
/// </para>
/// <para>
/// <b>Why it lives in the Submission tests.</b> The rows are ReferenceData's and
/// the only consumer is eCTD generation, which is here — beside the DTD checks
/// that answer the same question about the same package.
/// </para>
/// </remarks>
public sealed class FdaWireVocabularyTests
{
    /// <summary>
    /// A token RegOS holds that FDA does not publish is a package that is
    /// DTD-valid and rejected at the gateway — the exact failure the 2a/2b
    /// split was drawn to name.
    /// </summary>
    [Fact]
    public async Task EveryFdaApplicationTypeToken_IsOneFdaPublishes()
    {
        await using var ctx = New();

        var tokens = await ctx.ApplicationTypes
            .AsNoTracking()
            .Where(x => x.Token != null)
            .Select(x => new { x.Code, x.Token })
            .ToListAsync();

        var fdaTokens = tokens
            .Where(x => x.Token!.StartsWith("fdaat", StringComparison.Ordinal))
            .ToList();

        // If this is ever empty the test passes vacuously, which would hide the
        // seed being emptied rather than report it.
        fdaTokens.Should().NotBeEmpty();

        foreach (var row in fdaTokens)
        {
            Published.Should().ContainKey(
                row.Token!,
                "'{0}' is seeded on {1} and FDA's published list has no such code",
                row.Token,
                row.Code);

            Published[row.Token!].Should().Be(
                "active",
                "FDA marks '{0}' otherwise, and RegOS would be writing a retired code",
                row.Token);
        }
    }

    /// <summary>
    /// <b>E32 — a value list carrying obligations a schema cannot.</b>
    /// <c>fdaat7</c>, <c>fdaat9</c> and <c>fdaat10</c> are published and active,
    /// and the file's own comment says they *"should only be used in the
    /// cross-reference-application-number element"*; <c>fdaat8</c> says *"Do not
    /// use. For FDA use only"*.
    /// </summary>
    /// <remarks>
    /// The test above would happily accept all four, because FDA publishes all
    /// four as active. <b>This is the half no enumeration check can express</b>,
    /// and it is the one a future reader holding <c>application-type.xml</c> is
    /// most likely to "fix" — 510(k) and PMA are seeded rows sitting next to
    /// codes that plainly exist.
    /// </remarks>
    [Theory]
    [InlineData("fdaat7")]   // IDE          — cross-reference only
    [InlineData("fdaat9")]   // PMA          — cross-reference only
    [InlineData("fdaat10")]  // 510(k)       — cross-reference only
    [InlineData("fdaat8")]   // Safety Issue — FDA use only
    public async Task ACodeFdaReservesForOtherUses_IsNotSeededAsAnApplicationsOwnType(
        string reserved)
    {
        await using var ctx = New();

        // Published, active, and still not ours to write.
        Published.Should().ContainKey(reserved);

        var holders = await ctx.ApplicationTypes
            .AsNoTracking()
            .Where(x => x.Token == reserved)
            .Select(x => x.Code)
            .ToListAsync();

        holders.Should().BeEmpty(
            "FDA reserves '{0}' for a use that is not an application's own type",
            reserved);
    }

    /// <summary>
    /// <b>The absence that is evidence.</b> FDA's list is complete and
    /// <c>status</c>-flagged, so "no De Novo code" is a fact about eCTD rather
    /// than a gap in our reading — and the dev database holds a `FDA_DENOVO`
    /// application whose refusal now means something stronger than it did.
    /// </summary>
    [Fact]
    public void NoPublishedCode_MeansADeNovoRequest()
    {
        Published.Should().NotContainKey("fdaat11");

        Displays.Values.Should().NotContain(
            display => display.Contains("De Novo", StringComparison.OrdinalIgnoreCase),
            "the list would have to gain a code before RegOS could seed one");
    }

    // --- FDA's published list -------------------------------------------------

    private static readonly XDocument ApplicationTypeList =
        XDocument.Load(Path.Combine(
            RepositoryRoot(),
            "docs", "evidence", "EPIC-007a", "spec", "application-type.xml"));

    private static readonly IReadOnlyDictionary<string, string> Published =
        ApplicationTypeList.Root!.Elements("code-display")
            .ToDictionary(
                x => x.Attribute("code")!.Value,
                x => x.Attribute("status")!.Value);

    private static readonly IReadOnlyDictionary<string, string> Displays =
        ApplicationTypeList.Root!.Elements("code-display")
            .ToDictionary(
                x => x.Attribute("code")!.Value,
                x => x.Attribute("display")!.Value);

    /// <summary>
    /// Second occurrence of this walk — the architecture tests have their own,
    /// and it is <c>internal</c> to that assembly. ADR-018 says duplicate the
    /// second and extract on the third.
    /// </summary>
    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "RegOS.slnx")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            "Could not locate RegOS.slnx above " + AppContext.BaseDirectory
            + ". This test reads FDA's published value list out of "
            + "docs/evidence/ and cannot run outside the repository.");
    }

    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=regos;Username=admin;Password=password123";

    // ApplicationTypes are global — no tenant filter — but the context still
    // needs one to be constructed at all (ADR-031 is fail-closed).
    private static RegOSDbContext New() => new(
        new DbContextOptionsBuilder<RegOSDbContext>()
            .UseNpgsql(ConnectionString).Options,
        TestTenant.Context);
}
