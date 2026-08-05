using RegOS.TestSupport;

namespace RegOS.Api.Tests;

/// <summary>
/// This assembly's database — created from the current migration chain and
/// seeded by the real initializers before the host boots
/// (<see href="../../../docs/adr/ADR-064-the-test-suite-provisions-its-own-schema.md">ADR-064</see>).
/// </summary>
/// <remarks>
/// <b>Seeded twice, deliberately, and it costs nothing.</b> This fixture runs
/// the initializer chain, and then <c>Program</c> runs it again at boot — which
/// is exactly the insert-if-empty behaviour every initializer already has. It is
/// also the closest thing the suite has to a proof that booting against an
/// already-populated database changes nothing.
/// </remarks>
public sealed class ApiDatabase : RegOSTestDatabase;

/// <summary>
/// Puts every host test in one collection, which shares a single API host and
/// — the point — stops the classes running in parallel.
/// </summary>
/// <remarks>
/// They are not independent. Every class signs in as the one seeded development
/// account, so they share its refresh tokens and its sessions; run
/// concurrently, one class's cleanup deletes rows another is mid-way through
/// using. That is exactly what happened the first time these were run together.
///
/// The alternative — a distinct account per class — needs a way to create
/// accounts with passwords, which is what invitation acceptance has only just
/// provided. Worth revisiting once there is a second thing to isolate.
///
/// <para>
/// <b>One fixture, not two.</b> <see cref="ApiDatabase"/> is owned by
/// <see cref="RegOSApiFactory"/> rather than declared beside it, because xUnit 2
/// will not inject one collection fixture into another — see that class for what
/// the attempt costs.
/// </para>
/// </remarks>
[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<RegOSApiFactory>
{
    public const string Name = "RegOS API";
}
