using System.Text.RegularExpressions;

using FluentAssertions;

namespace RegOS.Architecture.Tests;

/// <summary>
/// SC-001 — every HTTP route lives under /api.
///
/// The prefix keeps the API in its own namespace on a host that also serves
/// health checks, OpenAPI and static files. RegOS started with it, and the
/// contexts written since have drifted off it one at a time, which is how the
/// same aggregate (Registration) ended up on two schemes at once.
/// </summary>
public sealed class RouteConventionTests
{
    /// <summary>
    /// A Map* call: captures the route literal. Verbatim strings are matched
    /// too, because a route containing an escape is still a route.
    /// </summary>
    private static readonly Regex MappedRoute = new(
        @"\.Map(?:Get|Post|Put|Patch|Delete)\(\s*@?""(?<route>[^""]*)""",
        RegexOptions.Compiled | RegexOptions.Singleline);

    /// <summary>
    /// A route group. Routes inside a group are relative to its prefix, so the
    /// prefix is what gets checked and the members are skipped.
    /// </summary>
    private static readonly Regex MappedGroup = new(
        @"\.MapGroup\(\s*@?""(?<prefix>[^""]*)""",
        RegexOptions.Compiled | RegexOptions.Singleline);

    /// <summary>
    /// Routes that predate this rule.
    ///
    /// This list may shrink and must never grow. Each entry is a live
    /// inconsistency, not an approved exception — moving one means changing
    /// the route and its frontend caller together, so they are being retired
    /// per context rather than in one sweep. Deleting the last entry should
    /// delete this list.
    ///
    /// See docs/engineering/slice-conventions.md § Route prefix.
    /// </summary>
    private static readonly HashSet<string> Grandfathered =
    [
        "/applications/{applicationId:guid}",
        "/applications/{applicationId:guid}/submissions",
        "/master-data/authorities",
        "/master-data/countries",
        "/reference-data/document-types",
        "/reference-data/templates",
        "/reference-data/templates/{id:guid}",
        "/registrations/{registrationId:guid}",
        "/registrations/{registrationId:guid}/approval",
        "/registrations/{registrationId:guid}/status",
        "/submission-types",
        "/submissions/{submissionId:guid}",
        "/submissions/{submissionId:guid}/attachable-documents",
        "/submissions/{submissionId:guid}/content-plan",
        "/submissions/{submissionId:guid}/documents",
        "/submissions/{submissionId:guid}/documents/{documentId:guid}/placement",
        "/submissions/{submissionId:guid}/documents/{submissionDocumentId:guid}",
        "/submissions/{submissionId:guid}/publish",
        "/submissions/{submissionId:guid}/snapshot",
        "/submissions/{submissionId:guid}/validation"
    ];

    [Fact]
    public void Every_route_is_under_the_api_prefix()
    {
        var offenders = new List<string>();

        foreach (var file in Repo.SourceFiles("src/Host/RegOS.Api/Endpoints"))
        {
            var code = Repo.CodeOf(file);

            // A grouped file delegates its prefix to MapGroup; check that
            // instead, and treat the members as relative paths.
            var groups = MappedGroup.Matches(code);

            var routes = groups.Count > 0
                ? groups.Select(m => m.Groups["prefix"].Value)
                : MappedRoute.Matches(code).Select(m => m.Groups["route"].Value);

            foreach (var route in routes)
            {
                if (route is "/api" || route.StartsWith("/api/", StringComparison.Ordinal))
                    continue;

                if (Grandfathered.Contains(route))
                    continue;

                offenders.Add($"{route}  ({Repo.Relative(file)})");
            }
        }

        offenders.Should().BeEmpty(
            "every RegOS route lives under /api (slice-conventions.md SC-001). "
            + "If this is a new route, prefix it. If you are moving an old one "
            + "off the grandfathered list, delete its entry there and update "
            + "the frontend call in the same commit");
    }

    /// <summary>
    /// Keeps the exemption list honest. An entry that no longer matches any
    /// route is either a fixed route or a typo, and both should be removed —
    /// otherwise the list slowly stops describing anything.
    /// </summary>
    [Fact]
    public void The_grandfathered_list_contains_no_stale_entries()
    {
        var live = Repo.SourceFiles("src/Host/RegOS.Api/Endpoints")
            .Select(Repo.CodeOf)
            .SelectMany(code => MappedRoute.Matches(code)
                .Select(m => m.Groups["route"].Value))
            .ToHashSet();

        var stale = Grandfathered.Where(route => !live.Contains(route)).ToList();

        stale.Should().BeEmpty(
            "these routes are exempted but no longer exist — delete them from "
            + "RouteConventionTests.Grandfathered");
    }
}
