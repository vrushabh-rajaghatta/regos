namespace RegOS.Api.Tests;

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
/// </remarks>
[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<RegOSApiFactory>
{
    public const string Name = "RegOS API";
}
