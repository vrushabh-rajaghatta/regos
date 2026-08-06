using System.Text.RegularExpressions;

using FluentAssertions;

namespace RegOS.Architecture.Tests;

/// <summary>
/// <b>Which bounded context may reference which — the architecture, written
/// down for the first time and executable.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>This is a specification, not a grandfathered list.</b> The distinction
/// matters more here than anywhere else in this project: a grandfather list says
/// *these are today's violations* and is shrink-only; this says *this is the
/// architecture*, and it changes when the architecture does — which
/// <c>CLAUDE.md</c> already requires be preceded by an ADR. Adding an edge here
/// is not defeat, it is the deliberate act. Adding one **without** an ADR is.
/// </para>
/// <para>
/// <b>Why it exists.</b> EPIC-010c added the first
/// <c>Product.Domain → Organization.Domain</c> reference and the architecture
/// suite stayed green, because none of its tests looked at the graph. The edge
/// was held by
/// <see href="../../../docs/adr/ADR-063-where-a-product-is-made-is-a-product-fact.md">ADR-063</see>
/// and a <c>.csproj</c> line. That ADR also closes the <em>reverse</em> edge
/// permanently — Organization must never reference Product — and nothing would
/// have stopped anyone opening it.
/// </para>
/// <para>
/// <b>What caught the equivalent one epic earlier was the compiler</b>, when
/// ADR-061 §3's proposed edge closed a cycle. <b>A cycle is self-enforcing; a
/// direction is not.</b> Every rule below is a direction.
/// </para>
/// </remarks>
public class ContextDependencyTests
{
    /// <summary>
    /// Referencable from anywhere, and deliberately unlisted per context.
    /// <c>SharedKernel</c> is ADR-017's scope; <c>Platform.Contracts</c> is
    /// ADR-041's identity that crosses; <c>Storage</c> is a driven port with no
    /// domain in it.
    /// </summary>
    private static readonly string[] Kernel =
        ["RegOS.SharedKernel", "RegOS.Platform.Contracts", "RegOS.Storage"];

    /// <summary>
    /// <b>The 32 edges.</b> Each key is a context whose <c>*.Domain</c> project
    /// may reference the <c>*.Domain</c> of every context listed, and no other.
    /// </summary>
    /// <remarks>
    /// Read down the list and the shape of RegOS is visible: <c>ReferenceData</c>
    /// and <c>Study</c> depend on nothing, because a country and a clinical study
    /// are facts about the world rather than about this business;
    /// <c>Interaction</c> depends on the most, because a health-authority
    /// conversation can be about anything RegOS holds.
    /// </remarks>
    private static readonly Dictionary<string, string[]> DomainMayReference = new()
    {
        // Facts about the world. Nothing beneath them.
        ["ReferenceData"] = [],
        ["Study"] = [],

        // Platform is tenancy and identity; it reaches the regulatory domain
        // through Contracts (ADR-041) and never directly.
        ["Platform"] = [],

        ["Organization"] = ["ReferenceData"],

        // ADR-065: Regulatory Process is an OPTIONAL bounded context. It consumes
        // the regulatory domain and is never its hub, so it is listed low here
        // rather than high.
        //
        // These three arrived one story at a time — ReferenceData with the
        // playbook (S001), then Product and RegulatoryApplication with the
        // objective (S002), which targets a product in a market and names the
        // application it is pursued through. The ADR authorises three more, all
        // INBOUND for the nullable ProcessStepId, and **none is declared until a
        // project takes it**: "the graph declares no edge that no project takes"
        // is what stops a permission outliving its reason.
        //
        // All three are now taken — Registration and Submission at S006,
        // Interaction at S007 — so the authorisation is spent. A fourth context
        // wanting a ProcessStepId is a new decision, not a remaining allowance.
        ["Process"] = ["Product", "ReferenceData", "RegulatoryApplication"],

        // ADR-063: where a product is made is a product fact. The reverse edge —
        // Organization → Product — is permanently closed, and the absence of
        // "Product" from Organization's list above is what now enforces it.
        ["Product"] = ["ReferenceData", "Organization"],

        ["ProductDocument"] = ["Product", "ReferenceData"],

        ["Labeling"] = ["Product", "ProductDocument", "ReferenceData"],

        ["RegulatoryApplication"] =
            ["Organization", "Product", "ReferenceData", "Study"],

        // ADR-061 §3: a pack authorisation lives here rather than in Product,
        // because the edge Product → Registration would have closed a cycle.
        // Process arrived at S006: a registration records which planned work
        // produced it (ADR-065 D2). An annotation, never ownership.
        ["Registration"] =
            ["Organization", "Process", "Product", "ReferenceData",
             "RegulatoryApplication"],

        ["Submission"] =
            ["Organization", "Process", "ProductDocument", "ReferenceData",
             "RegulatoryApplication", "Study"],

        // Process arrived at S007: all four interaction aggregates record which
        // planned work they serve. The third and last of the inbound edges
        // ADR-065 authorised — and the reason a conversation with an authority
        // now appears on the plan without the plan owning any of it.
        ["Interaction"] =
            ["Organization", "Process", "ReferenceData", "Registration",
             "RegulatoryApplication", "Submission"],
    };

    /// <summary>
    /// <b>The count in the summary above is load-bearing, so it is asserted.</b>
    /// It said 26 for five stories while the dictionary held 31 — a number in
    /// prose drifts silently, which is the one failure mode this whole test
    /// class exists to prevent.
    /// </summary>
    /// <remarks>
    /// Failing here is not a defect: adding an edge is meant to cost two edits,
    /// so that widening the graph is never something that happens by accident
    /// while doing something else. Update the number and the summary together.
    /// </remarks>
    [Fact]
    public void The_graph_holds_the_number_of_edges_it_says_it_does()
    {
        DomainMayReference.Sum(x => x.Value.Length).Should().Be(32,
            because: "the summary on DomainMayReference says 32 edges, and a "
                + "documented count that nothing checks is a count that drifts");
    }

    [Fact]
    public void A_domain_references_only_the_contexts_its_entry_names()
    {
        var offenders = new List<string>();

        foreach (var (project, references) in Projects().Where(x => Layer(x.Key) == "Domain"))
        {
            var context = Context(project);
            var allowed = DomainMayReference[context];

            foreach (var reference in references.Except(Kernel))
            {
                var target = Context(reference);

                if (Layer(reference) != "Domain" || !allowed.Contains(target))
                    offenders.Add($"{project} -> {reference}");
            }
        }

        offenders.Should().BeEmpty(
            "the dependency graph is a specification — add the edge to "
            + "DomainMayReference and write the ADR that CLAUDE.md requires for "
            + "a new cross-context dependency, or find another shape");
    }

    /// <summary>
    /// <b>Every entry is used.</b> An edge listed and not taken is a permission
    /// nobody asked for, and it would let the next real one through unnoticed.
    /// </summary>
    [Fact]
    public void The_graph_declares_no_edge_that_does_not_exist()
    {
        var declared = DomainMayReference
            .SelectMany(entry => entry.Value.Select(target => $"{entry.Key} -> {target}"))
            .ToList();

        var actual = Projects()
            .Where(x => Layer(x.Key) == "Domain")
            .SelectMany(x => x.Value
                .Except(Kernel)
                .Where(r => Layer(r) == "Domain")
                .Select(r => $"{Context(x.Key)} -> {Context(r)}"))
            .ToList();

        declared.Except(actual).Should().BeEmpty(
            "an edge that no project takes is a permission nobody asked for — "
            + "delete it, so the next real one has to be argued for");
    }

    /// <summary>
    /// <b>An application layer talks to its own domain and to persistence.</b>
    /// Never to another context's application, and never to another context's
    /// domain: a cross-context read goes through that context's own handlers, or
    /// it is a query over <c>RegOSDbContext</c> which
    /// <see href="../../../docs/adr/ADR-016-persistence-access-model.md">ADR-016</see>
    /// already governs.
    /// </summary>
    /// <remarks>
    /// This held with <b>zero exceptions</b> on the day it was written, across
    /// eleven contexts. Freezing something already true costs nothing; the value
    /// is entirely in the first attempt to break it.
    /// </remarks>
    [Fact]
    public void An_application_references_only_its_own_domain_and_persistence()
    {
        Offenders("Application", (context, reference) =>
                reference == "RegOS.Persistence"
                || reference == $"RegOS.{context}.Domain")
            .Should().BeEmpty(
                "an application project reaches another context through that "
                + "context's application surface, not by referencing its "
                + "internals");
    }

    /// <summary>
    /// <b>An infrastructure layer adds its own application to that, and nothing
    /// else.</b>
    /// </summary>
    /// <remarks>
    /// <b>Two projects broke this and both were redundant.</b>
    /// <c>Registration.Infrastructure</c> and
    /// <c>RegulatoryApplication.Infrastructure</c> each referenced
    /// <c>Product.Domain</c> for a product id type — which their own
    /// <c>*.Domain</c> already carried, so the references were removed and the
    /// solution built unchanged. **The graph got simpler rather than gaining two
    /// documented exceptions**, which is the better of the two outcomes this
    /// story could have had.
    /// </remarks>
    [Fact]
    public void An_infrastructure_references_only_its_own_slice_and_persistence()
    {
        Offenders("Infrastructure", (context, reference) =>
                reference == "RegOS.Persistence"
                || reference == $"RegOS.{context}.Domain"
                || reference == $"RegOS.{context}.Application")
            .Should().BeEmpty(
                "infrastructure implements its own context's ports; a reference "
                + "to another context's project is either redundant — check "
                + "whether your own Domain already carries it — or a design "
                + "question");
    }

    /// <summary>
    /// <b>The host composes; it does not reach past the application layer.</b>
    /// An endpoint that could see a <c>*.Domain</c> project could load an
    /// aggregate, and ADR-016 gives that job to a handler.
    /// </summary>
    [Fact]
    public void The_host_references_only_application_and_infrastructure()
    {
        var offenders = Projects()
            .Where(x => x.Key == "RegOS.Api")
            .SelectMany(x => x.Value)
            .Except(Kernel)
            .Where(reference => Layer(reference) is not ("Application" or "Infrastructure"))
            .ToList();

        offenders.Should().BeEmpty(
            "the host is a composition root — it wires application and "
            + "infrastructure together and reads neither's internals");
    }

    /// <summary>
    /// <b>A context owns only its own repositories</b> — checked in both
    /// directions.
    /// </summary>
    /// <remarks>
    /// <b>This is [ADR-065](../../../docs/adr/ADR-065-regulatory-process-is-an-optional-bounded-context.md)
    /// I2 made mechanical</b>, and it generalised on the way: the rule is true of
    /// every context, not only Process, and stating it narrowly would have been
    /// the special case.
    /// <para>
    /// <b>Cross-context <em>reads</em> compose over <c>RegOSDbContext</c>; cross-context
    /// <em>writes</em> never occur.</b> A write needs a repository, so a context that
    /// cannot name a foreign one cannot perform a foreign write — which is why
    /// this is a stronger guarantee than a review habit. ADR-016 already grants
    /// the read; this closes the write.
    /// </para>
    /// <para>
    /// <b>Both directions matter, and for different reasons.</b> Outbound
    /// (Process → <c>ISubmissionRepository</c>) would be Process taking ownership
    /// of a lifecycle that is not its own. Inbound
    /// (Submission → <c>IProcessPlanRepository</c>) would be a context reaching
    /// into Process to keep something "in sync" — the same mistake with the
    /// arrow reversed, and the one nobody thinks to look for.
    /// </para>
    /// <para>
    /// <b>It does not catch everything</b>, and the gap is worth naming: a
    /// handler could still call a domain method on an entity it read through
    /// <c>RegOSDbContext</c>. What it catches is the shape that actually arrives
    /// — a constructor taking a foreign repository — which is how every real
    /// version of this mistake has been written.
    /// </para>
    /// </remarks>
    [Fact]
    public void A_context_never_names_another_contexts_repository()
    {
        var offenders = new List<string>();

        foreach (var file in Repo.SourceFiles("src"))
        {
            var relative = Repo.Relative(file);
            var owner = ContextOfPath(relative);

            if (owner is null) continue;

            foreach (var match in RepositoryInterface.Matches(File.ReadAllText(file)))
            {
                var referenced = ContextOfNamespace(((Match)match).Groups[1].Value);

                if (referenced is not null && referenced != owner)
                    offenders.Add($"{relative} names {((Match)match).Value}");
            }
        }

        offenders.Should().BeEmpty(
            "a context owns only its own repositories (ADR-065 I2). Compose the "
            + "read over RegOSDbContext instead — and if you need a WRITE in "
            + "another context, that context's own command is where it belongs");
    }

    /// <summary>Matches a fully-qualified repository interface in a using or a type.</summary>
    private static readonly Regex RepositoryInterface = new(
        @"RegOS\.(\w+)\.Domain[\w.]*\.(I\w+Repository)", RegexOptions.Compiled);

    /// <summary><c>src/Process/RegOS.Process.Application/…</c> → <c>Process</c>.</summary>
    private static string? ContextOfPath(string relative)
    {
        var parts = relative.Split('/');

        return parts.Length > 2 && parts[0] == "src"
            && parts[1] is not ("Shared" or "Persistence" or "Host" or "Storage")
            ? parts[1]
            : null;
    }

    private static string? ContextOfNamespace(string context)
        => context is "SharedKernel" or "Storage" ? null : context;

    /// <summary>
    /// The negative control. Without it every assertion above passes by reading
    /// nothing, which is the failure mode this repository has been bitten by
    /// when counting test suites.
    /// </summary>
    [Fact]
    public void The_projects_are_actually_being_read()
    {
        var projects = Projects();

        projects.Should().HaveCountGreaterThan(30,
            "RegOS has eleven bounded contexts of up to three projects each, "
            + "plus the host, persistence and the kernel");

        projects.Count(x => Layer(x.Key) == "Domain").Should().Be(
            DomainMayReference.Count,
            "every context with a domain project needs an entry in the graph, "
            + "and an entry with no project is a context that was renamed or "
            + "removed without anyone updating this");
    }

    // --- reading the graph ---------------------------------------------------

    private static List<string> Offenders(
        string layer, Func<string, string, bool> permitted) =>
        Projects()
            .Where(x => Layer(x.Key) == layer)
            .SelectMany(x => x.Value
                .Except(Kernel)
                .Where(reference => !permitted(Context(x.Key), reference))
                .Select(reference => $"{x.Key} -> {reference}"))
            .ToList();

    private static readonly Regex Reference = new(
        @"ProjectReference\s+Include=""([^""]*\.csproj)""", RegexOptions.Compiled);

    /// <summary>
    /// Every project under <c>src/</c> and the RegOS projects it references, by
    /// assembly name. Read from the <c>.csproj</c> files rather than by
    /// reflection, for the reason
    /// <see cref="Repo"/> gives: a reflection test only sees the contexts this
    /// project references, and the context nobody wired in is the one that
    /// drifts.
    /// </summary>
    private static Dictionary<string, string[]> Projects()
    {
        var projects = new Dictionary<string, string[]>(StringComparer.Ordinal);

        foreach (var file in Directory.EnumerateFiles(
                     Path.Combine(Repo.Root, "src"), "*.csproj",
                     SearchOption.AllDirectories))
        {
            var references = Reference.Matches(File.ReadAllText(file))
                .Select(match => Path.GetFileNameWithoutExtension(
                    match.Groups[1].Value.Replace('\\', '/')))
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();

            projects[Path.GetFileNameWithoutExtension(file)] = references;
        }

        return projects;
    }

    /// <summary><c>RegOS.Registration.Infrastructure</c> → <c>Registration</c>.</summary>
    private static string Context(string project)
    {
        var parts = project.Split('.');

        return parts.Length >= 3 ? parts[1] : project;
    }

    /// <summary><c>RegOS.Registration.Infrastructure</c> → <c>Infrastructure</c>.</summary>
    private static string Layer(string project)
    {
        var parts = project.Split('.');

        return parts.Length >= 3 ? parts[^1] : string.Empty;
    }
}
