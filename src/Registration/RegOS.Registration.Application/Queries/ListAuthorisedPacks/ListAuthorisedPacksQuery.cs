using RegOS.Product.Domain.Product;

namespace RegOS.Registration.Application.Queries.ListAuthorisedPacks;

/// <summary>
/// <b>"Which packs are authorised in this market, and how are they supplied?"</b>
/// — the question EPIC-010b was cut to answer.
/// </summary>
/// <remarks>
/// <b>Keyed on the market, not on a licence</b>, and that is the decision. A
/// market has several licences and a pack may be authorised under more than one
/// of them — a partial divestment leaves exactly that — so asking a licence
/// which packs it authorises answers a narrower question than anybody has.
/// <para>
/// <b>Every pack is returned, authorised or not.</b> An unauthorised pack is
/// not an error and not a gap: a pack in design has no licence yet, and hiding
/// it would make the screen say the market sells less than it plans to. The
/// same call EPIC-018 S006 made about markets with a withdrawn indication —
/// return it, and say what its standing is.
/// </para>
/// </remarks>
public sealed record ListAuthorisedPacksQuery(MedicinalProductId MedicinalProductId);
