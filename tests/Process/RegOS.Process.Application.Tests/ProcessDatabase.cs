using RegOS.TestSupport;

namespace RegOS.Process.Application.Tests;

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
public sealed class ProcessDatabase : RegOSTestDatabase
{
    public const string Collection = "Process database";
}

/// <summary>
/// Puts every database-touching class in this assembly on one shared database,
/// which is also what stops them running in parallel with each other.
/// </summary>
[CollectionDefinition(ProcessDatabase.Collection)]
public sealed class ProcessDatabaseCollection : ICollectionFixture<ProcessDatabase>;
