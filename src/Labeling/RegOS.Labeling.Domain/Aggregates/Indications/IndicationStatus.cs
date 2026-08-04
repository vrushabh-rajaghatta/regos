namespace RegOS.Labeling.Domain.Aggregates.Indications;

/// <summary>
/// What an authority has decided about this indication.
/// </summary>
/// <remarks>
/// <b>These are successive regulatory decisions, not successive editions of a
/// document</b> — which is why an indication has a dated history and not a
/// revision. The discriminator: for a label the wording is the regulated
/// object; for an indication the approval is, and the wording is how the label
/// communicates it.
/// </remarks>
public enum IndicationStatus
{
    /// <summary>Authorised in this market.</summary>
    Approved = 0,

    /// <summary>Widened — a new population, a new line of therapy.</summary>
    Expanded = 1,

    /// <summary>Narrowed, and still authorised.</summary>
    Restricted = 2,

    /// <summary>No longer authorised. The record stays; the authorisation does not.</summary>
    Withdrawn = 3
}
