using RegOS.Product.Domain.Product;

namespace RegOS.Registration.Application.Queries.ListSiteAlignment;

/// <summary>
/// <b>"Where is this product made, and is that site on the licence?"</b> — the
/// question EPIC-010c was cut to answer, and the only place the two halves meet.
/// </summary>
/// <remarks>
/// <b>A read, and nothing but a read.</b> Neither side knows about the other:
/// <c>ManufacturingOperation</c> records what happens, <c>SiteApproval</c>
/// records what a licence permits, and they were built in separate contexts on
/// purpose. Introducing an "approved manufacturing operation" would have coupled
/// them and made this comparison impossible to state.
/// <para>
/// <b>It lives in Registration for the reason <c>ListAuthorisedPacks</c> does:</b>
/// this is the second read of the shape <em>compare what a licence permits
/// against what the product actually does</em>, and Registration is the context
/// that already reaches both (ADR-006).
/// </para>
/// </remarks>
public sealed record ListSiteAlignmentQuery(
    MedicinalProductId MedicinalProductId);
