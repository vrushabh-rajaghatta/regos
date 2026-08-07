using System.Text;
using System.Text.RegularExpressions;

using FluentAssertions;

namespace RegOS.Architecture.Tests;

/// <summary>
/// <b>The frontend calls routes the host actually maps.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>Why this exists.</b> EPIC-020 shipped seven stories in which every client
/// call the Process feature made returned 404 — the endpoints were declared
/// <c>/api/process-plans</c> and called as <c>/process-plans</c>. Fifteen files.
/// <c>npm run build</c> and <c>npm run lint</c> were green at every story and
/// structurally could not see it: a route is a string, not a type.
/// </para>
/// <para>
/// <b>The defect was never the missing <c>/api</c>.</b> It was that nothing
/// compared the two halves. Both halves are source, and the mismatch exists
/// before the application starts — so a static comparison tests the right
/// abstraction, and the architecture suite stays a suite of source tests rather
/// than becoming integration tests wearing a disguise.
/// </para>
/// <para>
/// <b>Its one assumption is itself asserted</b> — see
/// <see cref="Every_route_group_is_one_the_scanner_can_resolve"/>. A scanner
/// with an unchecked assumption is how the thing it is guarding against
/// happened in the first place.
/// </para>
/// </remarks>
public class ApiRouteAlignmentTests
{
    /// <summary>
    /// <b>Known SC-001 inconsistencies as at 2026-08-07, when S009 created this
    /// list. New exceptions must not be added.</b> Existing entries should only
    /// be removed — after normalising the route to <c>/api</c>, or after a
    /// documented architectural exception says why it stays.
    /// </summary>
    /// <remarks>
    /// <b>Shrink-only, and the direction is the whole point.</b> Making a
    /// failure go away by adding a prefix here defeats the guard exactly as
    /// adding to a grandfathered list defeats one — and these are not
    /// hypothetical: <b>this ambiguity is what made the Process bug look
    /// correct.</b> Both spellings appear in the codebase, so copying the
    /// nearest file was enough to be wrong.
    /// <para>
    /// Whether to normalise them or formally except them is an open decision,
    /// deliberately not taken by S009. Recording them is.
    /// </para>
    /// </remarks>
    private static readonly string[] PreExistingSc001Exceptions =
    [
        "/master-data",
        "/reference-data",
        "/submissions",
        "/applications",
        "/registrations",
        "/templates",
        "/countries",
        "/products",
        "/organizations",
        "/studies",
        "/substances"
    ];

    /// <summary>
    /// <b>Guard 1 — every client path reaches something.</b> The comparison the
    /// absence of which cost EPIC-020 seven stories.
    /// </summary>
    [Fact]
    public void Every_client_path_reaches_a_route_the_host_maps()
    {
        var routes = ServerRoutes();

        var offenders = ClientCalls()
            .Where(call => !routes.Any(route => Matches(call.Path, route)))
            .Select(call => $"{call.File}: {call.Path}")
            .Distinct()
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToList();

        offenders.Should().BeEmpty(
            "a client path that matches no mapped route is a 404 the type "
            + "system cannot see. Compare the endpoint's own Map* string — the "
            + "server is usually right and the client usually forgot /api");
    }

    /// <summary>
    /// <b>Guard 2 — the convention, not the current endpoints.</b>
    /// </summary>
    /// <remarks>
    /// Deliberately knows nothing about Process, or about any feature. It says
    /// what SC-001 says — <em>every route starts <c>/api</c></em> — from the
    /// client side, so a future feature cannot repeat this defect even against
    /// an endpoint that does not exist yet.
    /// </remarks>
    [Fact]
    public void Every_client_path_starts_with_api()
    {
        var offenders = ClientCalls()
            .Where(call => !call.Path.StartsWith("/api/", StringComparison.Ordinal))
            .Where(call => !PreExistingSc001Exceptions.Any(prefix =>
                call.Path == prefix
                || call.Path.StartsWith(prefix + "/", StringComparison.Ordinal)))
            .Select(call => $"{call.File}: {call.Path}")
            .Distinct()
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToList();

        offenders.Should().BeEmpty(
            "SC-001 — every route starts /api, and the client is half of that. "
            + "The exception list above is SHRINK-ONLY: adding to it to make "
            + "this pass is how a guard becomes decoration");
    }

    /// <summary>
    /// <b>Guard 3 — the scanner's own assumption, made executable.</b>
    /// </summary>
    /// <remarks>
    /// <see cref="ServerRoutes"/> resolves a group prefix only when the group is
    /// assigned to a variable and used <b>in the same file</b> — which is how
    /// every group in this codebase is written today, including the one group
    /// with a non-empty prefix (<c>TenantEndpoints</c>).
    /// <para>
    /// <b>The signed-off design said "assert every prefix is empty".</b> One
    /// already is not, and forcing it to be would have changed working code to
    /// suit a test. So the invariant asserted is the true one: <em>every
    /// prefix is one the scanner can see</em>. A group whose prefix is declared
    /// in one file and consumed in another would silently shorten every route
    /// the scanner derives from it, and silence is the failure mode this whole
    /// file exists to remove.
    /// </para>
    /// </remarks>
    [Fact]
    public void Every_route_group_is_one_the_scanner_can_resolve()
    {
        var offenders = new List<string>();

        foreach (var (relative, source) in EndpointSources())
        {
            var declared = GroupPrefixes(source);

            foreach (Match use in GroupedMapping.Matches(source))
            {
                var receiver = use.Groups["receiver"].Value;

                // `app` and `endpoints` are the extension-method parameter:
                // no prefix, and nothing to resolve.
                if (receiver is "app" or "endpoints") continue;

                if (!declared.ContainsKey(receiver))
                    offenders.Add($"{relative}: {receiver}.{use.Groups["verb"].Value}");
            }
        }

        offenders.Should().BeEmpty(
            "the source route scanner resolves a MapGroup prefix only within "
            + "the file that declares it. Update ServerRoutes() before mapping "
            + "endpoints onto a group declared elsewhere — otherwise every "
            + "route built on it is scanned without its prefix, and this file's "
            + "guarantees quietly stop holding");
    }


    /// <summary>
    /// <b>Guard 4 — a JSON body says it is JSON.</b>
    /// </summary>
    /// <remarks>
    /// <b>The second defect the browser proof found</b>, and the same shape as
    /// the first: seven Process writes passed <c>JSON.stringify(...)</c> with no
    /// headers, so <c>fetch</c> defaulted to <c>text/plain</c> and every one of
    /// them returned <b>415 Unsupported Media Type</b>. Invisible to the
    /// compiler, invisible to lint, and reached only by a browser.
    /// <para>
    /// <b>Keyed on <c>JSON.stringify</c> rather than on the presence of a
    /// body</b>, which is what makes it safe for uploads:
    /// <c>uploadProductDocument</c> posts <c>FormData</c> and <em>must</em> omit
    /// the header so the browser can set the multipart boundary. A guard that
    /// demanded a header on every body would have been wrong there, and being
    /// wrong once is how a guard gets an exception list.
    /// </para>
    /// </remarks>
    [Fact]
    public void Every_json_body_declares_its_content_type()
    {
        var offenders = new List<string>();

        foreach (var file in Directory.EnumerateFiles(
                     Path.Combine(Repo.Root, "web", "regos-web", "src"),
                     "*.ts", SearchOption.AllDirectories))
        {
            var source = File.ReadAllText(file);

            foreach (Match request in JsonBodyRequest.Matches(source))
            {
                if (!request.Value.Contains("Content-Type", StringComparison.Ordinal))
                    offenders.Add(
                        Path.GetRelativePath(Repo.Root, file).Replace('\\', '/'));
            }
        }

        offenders.Distinct().Should().BeEmpty(
            "a JSON body with no Content-Type is sent as text/plain and the API "
            + "answers 415. Add headers: { \"Content-Type\": \"application/json\" } "
            + "— and note FormData uploads must NOT have one, which is why this "
            + "guard keys on JSON.stringify");
    }

    /// <summary>
    /// Whether a client path could reach a mapped route.
    /// </summary>
    /// <remarks>
    /// <b>Segment-wise, because a client interpolates two different things into
    /// the same syntax.</b> <c>${planId}</c> is a parameter value, but
    /// <c>${kind}</c> in <c>/api/medicinal-products/${id}/${kind}</c> is a
    /// literal segment chosen at runtime — <c>indications</c> or
    /// <c>contraindications</c> — and a string comparison flags it as a 404 it
    /// is not. Five such calls, none of them defects.
    /// <para>
    /// So a <c>{}</c> on either side matches anything on the other. <b>It costs
    /// nothing this file exists for</b>: the defect it was built for is a
    /// missing <c>/api</c> prefix, and a prefix is a literal segment on both
    /// sides — never a <c>{}</c>. Guard 2 checks that class outright and is
    /// unaffected by any of this.
    /// </para>
    /// </remarks>
    private static bool Matches(string clientPath, string route)
    {
        var client = clientPath.Split('/');
        var server = route.Split('/');

        if (client.Length != server.Length) return false;

        return !client.Where((segment, i) =>
            segment != server[i] && segment != "{}" && server[i] != "{}").Any();
    }

    // --- the scanner ---------------------------------------------------------

    /// <summary>
    /// Every route the host maps, with parameters normalised to <c>{}</c> and
    /// any group prefix applied.
    /// </summary>
    private static HashSet<string> ServerRoutes()
    {
        var routes = new HashSet<string>(StringComparer.Ordinal);

        foreach (var (_, source) in EndpointSources())
        {
            var prefixes = GroupPrefixes(source);

            foreach (Match mapping in GroupedMapping.Matches(source))
            {
                var prefix = prefixes.GetValueOrDefault(
                    mapping.Groups["receiver"].Value, string.Empty);

                routes.Add(Normalise(prefix + Concatenated(mapping.Groups["route"].Value)));
            }
        }

        return routes;
    }

    /// <summary>
    /// Every path the frontend asks <c>buildUrl</c> for, normalised the same
    /// way — so the two sides are compared as the same kind of thing.
    /// </summary>
    private static IEnumerable<(string File, string Path)> ClientCalls()
    {
        var web = Path.Combine(Repo.Root, "web", "regos-web", "src");

        foreach (var file in Directory.EnumerateFiles(
                     web, "*.ts", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(Repo.Root, file)
                .Replace('\\', '/');

            var source = File.ReadAllText(file);

            foreach (Match call in BuildUrlCall.Matches(source))
            {
                var literal = ReadTemplate(source, call.Index + call.Length);

                if (literal is not null)
                    yield return (relative, Normalise(literal));
            }
        }
    }

    /// <summary>
    /// Reads a <c>"…"</c> or <c>`…`</c> literal, replacing every
    /// <c>${…}</c> — including one containing nested braces or its own template
    /// — with a single <c>{}</c>. Hand-written rather than a regex because
    /// <c>${asOf ? `?asOf=${asOf}` : ""}</c> defeats one.
    /// </summary>
    private static string? ReadTemplate(string source, int start)
    {
        if (start >= source.Length) return null;

        var quote = source[start];

        if (quote is not ('"' or '`' or '\'')) return null;

        var text = new StringBuilder();

        for (var i = start + 1; i < source.Length; i++)
        {
            if (source[i] == quote) return text.ToString();

            if (source[i] == '$' && i + 1 < source.Length && source[i + 1] == '{')
            {
                var depth = 1;
                i += 2;

                while (i < source.Length && depth > 0)
                {
                    if (source[i] == '{') depth++;
                    else if (source[i] == '}') depth--;
                    i++;
                }

                i--;
                text.Append("{}");
                continue;
            }

            text.Append(source[i]);
        }

        return null;
    }

    /// <summary>
    /// <c>/api/plans/{id:guid}/steps</c> and
    /// <c>/api/plans/${planId}/steps</c> become the same string.
    /// </summary>
    /// <remarks>
    /// A trailing <c>{}</c> glued to a segment rather than occupying one is an
    /// interpolated query string — <c>/impact${asOf ? "?asOf=…" : ""}</c> — and
    /// is dropped. Route parameters always occupy a whole segment here.
    /// </remarks>
    private static string Normalise(string path)
    {
        var normalised = RouteParameter.Replace(path, "{}");

        var query = normalised.IndexOf('?', StringComparison.Ordinal);

        if (query >= 0) normalised = normalised[..query];

        normalised = GluedSuffix.Replace(normalised, "$1");

        normalised = normalised.TrimEnd('/');

        return normalised.Length == 0 ? "/" : normalised;
    }

    /// <summary>Joins <c>"a" + "b"</c> back into the one string it compiles to.</summary>
    private static string Concatenated(string literals)
        => string.Concat(StringLiteral.Matches(literals)
            .Select(match => match.Groups["text"].Value));

    /// <summary><c>var tenants = app.MapGroup("/api/platform/tenants")</c></summary>
    private static Dictionary<string, string> GroupPrefixes(string source)
        => GroupDeclaration.Matches(source).ToDictionary(
            m => m.Groups["name"].Value,
            m => m.Groups["prefix"].Value,
            StringComparer.Ordinal);

    private static IEnumerable<(string Relative, string Source)> EndpointSources()
    {
        var host = Path.Combine(Repo.Root, "src", "Host", "RegOS.Api");

        foreach (var file in Directory.EnumerateFiles(
                     host, "*.cs", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(Repo.Root, file)
                .Replace('\\', '/');

            if (relative.Contains("/obj/", StringComparison.Ordinal)
                || relative.Contains("/bin/", StringComparison.Ordinal))
                continue;

            yield return (relative, File.ReadAllText(file));
        }
    }

    private static readonly Regex GroupDeclaration = new(
        @"var\s+(?<name>\w+)\s*=\s*\w+\s*\.\s*MapGroup\(\s*""(?<prefix>[^""]*)""",
        RegexOptions.Compiled);

    /// <summary>
    /// <c>app.MapDelete("/api/x/{id:guid}" + "/y/{other:guid}", …)</c> — the
    /// route is one string to the compiler and two literals in the source, so
    /// the trailing <c>(\s*\+\s*"…")*</c> is not optional decoration. One
    /// endpoint is written this way and reading only its first literal reported
    /// a working route as a 404.
    /// </summary>
    private static readonly Regex GroupedMapping = new(
        @"(?<receiver>\w+)\s*\.\s*(?<verb>Map(?:Get|Post|Put|Patch|Delete))\(\s*"
        + @"(?<route>""[^""]+""(?:\s*\+\s*""[^""]+"")*)",
        RegexOptions.Compiled);

    /// <summary>
    /// A <c>fetch</c>/<c>apiFetch</c> options object carrying a
    /// <c>JSON.stringify</c> body. Non-greedy to the closing brace of the
    /// options literal.
    /// </summary>
    private static readonly Regex JsonBodyRequest = new(
        @"\{[^{}]*?body:\s*JSON\.stringify[^{}]*?\}|\{(?:[^{}]|\{[^{}]*\})*?body:\s*JSON\.stringify(?:[^{}]|\{[^{}]*\})*?\}",
        RegexOptions.Compiled | RegexOptions.Singleline);

    private static readonly Regex StringLiteral = new(
        @"""(?<text>[^""]*)""", RegexOptions.Compiled);

    private static readonly Regex BuildUrlCall = new(
        @"buildUrl\(\s*", RegexOptions.Compiled);

    private static readonly Regex RouteParameter = new(
        @"\{[^}]*\}", RegexOptions.Compiled);

    /// <summary><c>/impact{}</c> — a <c>{}</c> that does not follow a slash.</summary>
    private static readonly Regex GluedSuffix = new(
        @"([^/]){\}", RegexOptions.Compiled);
}
