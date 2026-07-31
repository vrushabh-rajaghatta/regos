using System.Text.RegularExpressions;

using FluentAssertions;

namespace RegOS.Architecture.Tests;

/// <summary>
/// SC-004 — an endpoint's handler is a named method, not an inline lambda.
///
/// The route line then reads as a table of contents — path, verb, handler
/// name — and the handler itself is a normal method that can be read, moved
/// and given a comment explaining what the capability does. A lambda buries
/// all three inside the registration call.
/// </summary>
public sealed class EndpointConventionTests
{
    /// <summary>
    /// A Map* call whose handler argument opens a lambda — either
    /// <c>async (…) =></c> or <c>(…) =></c> — rather than naming a method.
    /// The route literal is optional so grouped routes are covered too.
    /// </summary>
    private static readonly Regex LambdaHandler = new(
        @"\.Map(?:Get|Post|Put|Patch|Delete)\(\s*(?:@?""[^""]*""\s*,)?\s*(?:async\s*)?\(",
        RegexOptions.Compiled | RegexOptions.Singleline);

    /// <summary>
    /// Endpoints written as lambdas before this rule. Shrink, never grow.
    /// </summary>
    private static readonly HashSet<string> Grandfathered =
    [
    ];

    [Fact]
    public void Endpoint_handlers_are_named_methods()
    {
        var offenders = Repo.SourceFiles("src/Host/RegOS.Api/Endpoints")
            .Where(file => LambdaHandler.IsMatch(Repo.CodeOf(file)))
            .Select(Repo.Relative)
            .Where(path => !Grandfathered.Contains(path))
            .ToList();

        offenders.Should().BeEmpty(
            "register the route against a named static method — see "
            + "GetProductEndpoint for the shape (slice-conventions.md SC-004)");
    }

    [Fact]
    public void The_grandfathered_list_contains_no_stale_entries()
    {
        var stale = Grandfathered
            .Where(path =>
            {
                var full = Path.Combine(Repo.Root, path);
                return !File.Exists(full) || !LambdaHandler.IsMatch(Repo.CodeOf(full));
            })
            .ToList();

        stale.Should().BeEmpty(
            "these files no longer use lambda handlers — delete them from "
            + "EndpointConventionTests.Grandfathered");
    }
}
