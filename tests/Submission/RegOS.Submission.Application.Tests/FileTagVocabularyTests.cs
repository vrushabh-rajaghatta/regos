using System.Xml.Linq;

using FluentAssertions;

using RegOS.Submission.Application.StudyTagging;

namespace RegOS.Submission.Application.Tests;

/// <summary>
/// EPIC-019 S002b — the `file-tag` table in code is ICH's published list.
/// </summary>
/// <remarks>
/// <b>Level 2a applied to a table we wrote.</b> The vocabulary lives in code
/// (<c>FileTagVocabulary</c>) rather than in the database, so the thing that
/// could rot is a transcription rather than a seed — and the check is the same
/// either way: read the normative artifact and compare. The same move
/// <c>FdaWireVocabularyTests</c> makes for `application-type`.
/// <para>
/// It also carries the assumption the design rests on. All 97 published values
/// are distinct across <c>ich</c>, <c>us</c> and <c>jp</c>, which is why a
/// placement stores the tag alone and derives its realm. <b>If ICH ever
/// publishes the same value in two realms, that derivation silently starts
/// lying</b>, and this test is what notices.
/// </para>
/// </remarks>
public sealed class FileTagVocabularyTests
{
    private static XElement Published()
    {
        var path = Path.Combine(
            RepositoryRoot(),
            "docs", "evidence", "EPIC-019", "spec", "valid-values.xml");

        File.Exists(path).Should().BeTrue(
            "the held vocabulary is the authority for this table (E33); "
            + $"expected it at {path}");

        return XDocument.Load(path).Root!.Element("file-tag")!;
    }

    private static IReadOnlyList<(string Value, string Realm)> PublishedTags() =>
        Published()
            .Elements("valid-value")
            .Select(v => (
                Value: v.Attribute("value")!.Value,
                Realm: v.Attribute("realm")!.Value))
            .ToList();

    [Fact]
    public void TheTable_IsExactlyWhatIchPublishes()
    {
        var published = PublishedTags();

        FileTagVocabulary.AsMap.Should().HaveCount(published.Count);

        foreach (var (value, realm) in published)
        {
            FileTagVocabulary.Contains(value).Should().BeTrue(
                $"ICH publishes \"{value}\" and the table omits it");

            FileTagVocabulary.RealmOf(value).Should().Be(realm,
                $"\"{value}\" is published under info-type=\"{realm}\", and "
                + "emitting the wrong one produces a file the DTD accepts and "
                + "the ICH stylesheet paints red (E34)");
        }
    }

    [Fact]
    public void NothingIsInTheTableThatIchDoesNotPublish()
    {
        var published = PublishedTags().Select(t => t.Value).ToHashSet();

        FileTagVocabulary.All.Where(tag => !published.Contains(tag))
            .Should().BeEmpty(
                "a tag RegOS offers and ICH does not publish is one a filer "
                + "can pick and a reviewer's tool will not recognise");
    }

    /// <summary>
    /// The assumption behind storing one column instead of two.
    /// </summary>
    [Fact]
    public void EveryValue_IsDistinctAcrossRealms_SoTheRealmIsDerivable()
    {
        var published = PublishedTags();

        published.Select(t => t.Value).Should().OnlyHaveUniqueItems(
            "a placement stores the tag and derives info-type from it, which "
            + "only holds while no value appears in two realms — if this "
            + "fails, the placement needs a realm column and S003 needs to "
            + "know which realm was chosen");
    }

    [Fact]
    public void TheCountsAreTheOnesRecordedAsEvidence()
    {
        var byRealm = PublishedTags()
            .GroupBy(t => t.Realm)
            .ToDictionary(g => g.Key, g => g.Count());

        // E33. Written out rather than derived, so that a vocabulary update
        // arrives as a failing test with a number to read rather than as a
        // silent change.
        byRealm.Should().BeEquivalentTo(new Dictionary<string, int>
        {
            ["ich"] = 68,
            ["us"] = 25,
            ["jp"] = 4
        });
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null
            && !Directory.Exists(Path.Combine(directory.FullName, "docs")))
        {
            directory = directory.Parent;
        }

        directory.Should().NotBeNull("the test runs inside the repository");

        return directory!.FullName;
    }
}
