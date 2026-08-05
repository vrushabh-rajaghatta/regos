using RegOS.TestSupport;

namespace RegOS.Submission.Application.Tests;

/// <summary>
/// This assembly's database — created from the current migration chain, seeded
/// by the real initializers, dropped when the assembly's tests finish
/// (<see href="../../../docs/adr/ADR-064-the-test-suite-provisions-its-own-schema.md">ADR-064</see>).
/// </summary>
/// <remarks>
/// A one-line subclass rather than the base type directly, so that
/// <c>GetType().Assembly</c> inside <see cref="RegOSTestDatabase"/> names
/// <em>this</em> assembly and the database is named after it.
/// </remarks>
public sealed class SubmissionDatabase : RegOSTestDatabase
{
    public const string Collection = "Submission database";
}

/// <summary>
/// Puts every database-touching class in this assembly on one shared database.
/// </summary>
/// <remarks>
/// <b>They therefore do not run in parallel with each other</b>, which is the
/// price of one database per assembly and is worth naming rather than
/// discovering. It is also what the hand-written cleanup in these tests has
/// always assumed.
/// </remarks>
[CollectionDefinition(SubmissionDatabase.Collection)]
public sealed class SubmissionDatabaseCollection : ICollectionFixture<SubmissionDatabase>;
