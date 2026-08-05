using RegOS.Product.Domain.Product;

namespace RegOS.Registration.Application.Queries.ListApprovedSites;

/// <summary>
/// <b>"Which sites do this market's licences approve?"</b>
/// </summary>
/// <remarks>
/// <b>Keyed on the market, not on a licence</b>, and that is the decision — the
/// same one <c>ListAuthorisedPacksQuery</c> made. A market holds several
/// licences and a site may be named on more than one of them, so asking a
/// licence which sites it approves answers a narrower question than anybody
/// has. It is also the shape S004 needs: the divergence compares what a
/// <em>market</em> approves against what its product's operations say.
/// </remarks>
public sealed record ListApprovedSitesQuery(
    MedicinalProductId MedicinalProductId);
